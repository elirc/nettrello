# Concepts: PATCH presence and aggregate safety

Nullable values and optional fields are different ideas. An omitted description means preserve the old description. A JSON `null` description means clear it. A missing `Priority` means preserve the current enum; an unknown string such as `Imaginary` is a bad request. Nonnullable `JsonElement` observes three states: `Undefined` for omission, `Null` for clearing, and a value kind for replacement. Nullable `JsonElement?` loses that distinction during deserialization because JSON null becomes CLR null. `UpdateCardRequest.ResolveAgainst` reads all fields and produces a complete `CardUpdateValues` before `Card.Update` starts.

Validation must precede mutation at every domain boundary. A card update first computes required title, optional description, and normalized labels in local variables. If a late label is empty or too long, no property has been assigned. A move first resolves both source and target columns and validates the nonnegative position. A full target is rejected before the source card is removed. Same-column reorder is a distinct operation because moving a card within a full column does not increase cardinality. `BoardColumn.RepositionCard` removes and reinserts only after position validation, then reindexes.

The repository adds another boundary. A callback receives a clone, and only a callback that returns completes storage. A failed move therefore leaves the stored board unchanged even when the callback had begun work on its clone. This protects persistence from exceptions but does not excuse domain code that mutates before validation: callers can still observe a corrupted in-memory clone, timestamps can change, and later callback logic can make a wrong decision. The design uses both defenses. Version checks run under the same write lock; two requests with one expected version produce one success and one `409` conflict.


The version value is an aggregate-level precondition, not a field-level merge. A request with version 4 cannot silently apply to version 5 because the client may have reordered cards or changed capacity assumptions. The in-memory repository checks the value while its semaphore is held, so the conflict decision and replacement are serialized. This is different from a UI-only disabled button: a caller can still send an old header, and the service remains responsible for rejecting it. The fixed clock in the checks makes timestamp preservation observable rather than dependent on wall-clock timing.


One subtle point is that the parser validates all supplied fields before the domain update, but omitted fields are still resolved from a current clone. That means a caller cannot use omission to bypass a required title rule: the existing title is carried forward and remains valid. An explicit blank title reaches `Guard.Required` and fails. Similarly, labels are validated even when the title itself is valid. Validation order is designed to avoid assignments, not to promise a particular error when several fields are wrong.


## The `JsonValueKind` decision table

Every PATCH field in `UpdateCardRequest` is a non-nullable `JsonElement`, which means the deserializer can tell you three different things about it. Reading that off the `ValueKind` is the whole mechanism:

| `ValueKind` | What the client sent | Decision | Example |
|---|---|---|---|
| `Undefined` | the property was **not in the JSON at all** | keep the current value | `{"title":"x"}` leaves `description` alone |
| `Null` | the property was present with an explicit `null` | clear it, if the field is nullable; otherwise 400 | `{"description":null}` clears; `{"title":null}` is `card.field_invalid` |
| `String` | a text value | validate and set (parse for enums, `DateTimeOffset`, `Guid`) | `{"priority":"Low"}` |
| `Number` | a numeric value | 400 for a field that expects text | `{"dueDate":42}` is `card.field_invalid` |
| `Array` | a list | validate every element, then set | `{"labels":["a","b"]}` sets; `{"labels":["ok",4]}` is `card.labels_invalid` |
| `True`/`False`/`Object` | something else entirely | 400 | any unmodelled shape |

Plus one case that never reaches your code: an **unknown property**. `[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]` makes `System.Text.Json` throw a `JsonException` during deserialization, so `{"titel":"typo"}` is a 400 rather than a silently ignored no-op. That default — ignore unknown members — is the reason so many APIs quietly discard a client's misspelled field.

### The two alternatives you will meet in the wild

`JsonElement` is not the only way to model "omitted versus null". Know all three and when each is right:

**1. An optional wrapper DTO.** Define a small struct that records whether a value was supplied:

```csharp
public readonly struct Optional<T>
{
    public bool HasValue { get; init; }   // was the property present at all?
    public T? Value { get; init; }        // its value, possibly null
}

public sealed class UpdateCardDto
{
    public Optional<string> Title { get; init; }
    public Optional<string?> Description { get; init; }
}
```

You need a `JsonConverter<Optional<T>>` to populate it (the converter is only called for properties that *are* present, which is exactly how `HasValue` becomes true). **When to use it:** when you want the rest of your code to be strongly typed — `Optional<DateTimeOffset?>` rather than a `JsonElement` you must parse by hand — and when the same DTO is validated by FluentValidation or DataAnnotations. It is the most readable option in a codebase with many PATCH endpoints, at the cost of the converter and one generic type.

**2. `JsonPatchDocument` / RFC 6902.** The standardised patch format, where the body is a list of operations rather than a partial object:

```json
[ { "op": "replace", "path": "/title", "value": "Updated title" },
  { "op": "remove",  "path": "/dueDate" } ]
```

with `Microsoft.AspNetCore.JsonPatch` and `patch.ApplyTo(card, ModelState)`. **When to use it:** when clients need operations a partial object cannot express — `add`/`remove` on array elements, `move`, `copy`, or the `test` operation for optimistic concurrency on a single field. **When not to:** for ordinary "change these two fields" edits, because it is harder for clients to construct, harder to validate (the operations apply to your entity directly unless you patch a DTO first), and unfamiliar to most front-end developers. Note also that RFC 7396 *JSON Merge Patch* is a third standard, and it defines `null` as "delete" — which is exactly the convention this project implements by hand.

What all three share is the actual lesson: **a partial update must distinguish "not mentioned" from "set to nothing", and it must validate every mentioned field before it mutates anything.** The mechanism is a detail; getting that distinction wrong turns a rename into silent data loss.

## From an in-memory board to a database

`InMemoryBoardRepository` gives this project two guarantees a dictionary does not normally have: a `SemaphoreSlim` around read-modify-write, and a deep clone on every read so a caller who mutates the returned board cannot corrupt stored state. Both are stand-ins for things a database does differently, and you should be able to name the translation:

| Here | With EF Core and a real database |
|---|---|
| `SemaphoreSlim` around the whole update | A transaction plus a concurrency token. `Version` becomes `[Timestamp] public byte[] RowVersion { get; set; }` (SQL Server) or `.IsRowVersion()`/`xmin` (PostgreSQL); EF appends `WHERE RowVersion = @original` to the `UPDATE`, and a zero-row result raises `DbUpdateConcurrencyException`. A semaphore protects one process; a token protects every process on every machine. |
| The explicit `expectedVersion` check throwing `board.version_conflict` | The same `DbUpdateConcurrencyException`, surfaced to the client as **412 Precondition Failed** with `If-Match`, or **409 Conflict** for a version field in the body. Alternatively an explicit `ExecuteUpdateAsync` with `.Where(b => b.Version == expected)` and an affected-row count of 0. |
| Clone-on-read | `AsNoTracking()` for reads you will not save. Without it, EF's change tracker is holding the same instance you are mutating, and the next `SaveChangesAsync` writes your "temporary" change. The clone here is teaching you the habit the tracker will otherwise punish. |
| Board + columns + cards saved as one object | One `SaveChangesAsync` inside one transaction. The aggregate boundary you see in the domain is exactly the transaction boundary you want in the database. |
| Card `Position` reindexed in a list | An `int Position` column updated for the affected rows in the same transaction, with a unique index on `(ColumnId, Position)` if you want the database to refuse a duplicate ordering — which then forces you to reindex deliberately instead of by accident. |
| `CardLimit` checked in `BoardColumn` | Still checked in the domain, because a `CHECK` constraint cannot count sibling rows portably; but the count and the insert must be in one transaction, or two concurrent adds both pass the check. This is the same "check-then-act" race the version token solves for updates. |

Nothing here is implemented against a database, which is why every assertion in chapter 04 is about in-process state rather than durability.
