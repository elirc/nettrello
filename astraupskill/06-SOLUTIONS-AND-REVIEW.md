# Solutions and review

Exercise 1: title-only PATCH preserves all five omitted values because `ResolveAgainst` returns current values for undefined elements. Explicit description or labels null becomes a clear, represented as empty description or an empty label list by the domain normalizer. The complete React editor still sends all fields, so it follows the same path without a migration.

Exercise 2: `{}` returns 400 `card.patch_empty`; unknown priority returns 400 `card.priority_invalid`; null title returns 400 `card.field_invalid`; a non-string label returns 400 `card.labels_invalid`. Each error is raised while resolving the request, before `Card.Update` or `Board.Touch`, so the aggregate version and timestamp stay unchanged. The exception middleware serializes the problem code as JSON. The HTTP harness verifies fields and status; it does not assert Content-Type.

Exercise 3: missing target is checked before removal, full target is checked before removal, and negative position is checked before either. Every rejected snapshot equals the prior snapshot. Exercise 4 uses `RepositionCard`: remove and reinsert within the same list, reindex, then touch the board. Capacity does not increase. The move changes the board version only on success.

Exercise 5 runs two `MutateAsync` calls with one expected version. `SemaphoreSlim` admits one callback; it stores a clone and increments version. The next callback sees a different version and throws `board.version_conflict`. Exercise 6 passes because `GetAsync` returns `stored.Clone()` and `UpdateAsync` also clones before storing. This clone boundary protects failed callbacks, while prevalidation protects the clone itself.

Review limits are explicit. The repository is process-local and has no database durability or cross-process lock. The HTTP script uses a local seeded server, not browser automation or production authentication. The parser rejects unknown values but does not define a future versioned wire schema. A production implementation would add a database transaction, optimistic concurrency at the database boundary, and integration tests across workers.


A reviewer should look for accidental regressions in successful paths too. A complete request containing all six properties must still parse, preserve a valid assignee, normalize label order, and return a touched timestamp. A same-column reorder should return cards indexed from zero and increment board version once. A cross-column move should remove from the source and insert into the target exactly once. The tests therefore check positive results as well as rejection snapshots. They do not assert a particular generated GUID or wall-clock value; those are implementation details. The important identity assertions are card ids, source/target columns, and expected-version outcomes.


The solution deliberately does not add a second lock to the domain. A domain object should validate its own invariants, while the repository controls publication and version serialization. Two locks would make the teaching example harder to reason about and would still not coordinate another process. Instead, the reviewer can see the three boundaries directly: transport parsing, aggregate validation, and repository publication. Each has a test whose observation point matches its responsibility.


A complete review table for one card makes the contract concrete:

| Request body | Resolver result | Domain action | HTTP result | Version |
| --- | --- | --- | --- | --- |
| `{}` | no fields present | none | 400 `card.patch_empty` | unchanged |
| `{"title":"Renamed"}` | old values fill omitted fields | validate and assign title | 200 | plus one |
| `{"description":null}` | explicit clear, old values elsewhere | normalize to empty description | 200 | plus one |
| `{"priority":"Imaginary"}` | unknown enum | none | 400 `card.priority_invalid` | unchanged |
| `{"labels":["ok",4]}` | invalid element type | none | 400 `card.labels_invalid` | unchanged |

A complete move review table has the same shape. Missing destination, full destination, and negative position all stop before source removal. A same-column reorder removes and reinserts within one list, reindexes positions, and touches the board once. A cross-column move validates the target limit, removes from the source, assigns the requested timestamp to the moved card, inserts into the target, and touches once. The target's existing order remains stable except for the inserted card and subsequent reindexing. These are observable rules, not internal implementation trivia, because the client renders card order and the API returns board version.

The test harness intentionally checks an invalid label after valid title, description, priority, assignee, and due-date inputs. This catches a subtle class of regression: a developer can write the obvious assignments first and leave label normalization last. The exception would then be correct, but the object would retain a new title and timestamp. Precomputed locals fix that class of defect. The same reasoning applies to moves: resolving the target after removal creates a correct `column.not_found` code with an incorrect aggregate. The safe sequence validates every value that can throw, then performs the mutation.

For HTTP, the middleware's output is part of the contract. A status-only assertion could pass if the response body were empty or an HTML server error. The script parses JSON and checks the machine-readable `code`, while the service harness checks domain codes directly. Keep those tests separate: a domain exception proves invariant behavior, and a serialized HTTP problem proves middleware mapping. Neither test claims that a browser handles the error well; that would require a client test and is outside this bounded change.

Exercise 7: use direct domain assertions for failed moves and late label validation; service assertions for resolved PATCH values; repository reads for clone isolation and expected-version conflicts; and the HTTP script for status and serialized problem codes. The console harness groups scenarios into one executable, so do not count every assertion as a separate unit test. Its same-column success uses the unlimited source column; a reorder in a full column is additional practice, not a captured separate check. The Describe helper includes columns and card fields but omits board Version and UpdatedAt, so those failed-write observations need additional assertions when extending it.

For exercise 1, this is the actual tested null-clear block. Read it as `CardPatchTests.ExplicitNulls_ClearNullableFields_AndAStringPriorityIsParsed` in tests/NetTrello.Tests (or, in the delivered console harness, in tests/NetTrello.Checks/Program.cs after the shared fixture and title-only update). Request performs JsonSerializer deserialization with web defaults. Imports, fixture and helper definitions are in that linked executable; this block is not standalone.

```csharp
var clears = Request("{\"description\":null,\"assigneeId\":null,\"dueDate\":null,\"labels\":null,\"priority\":\"Low\"}");
updated = await service.UpdateCardAsync(board.Id, first.Id, clears, null, CancellationToken.None);
updatedCard = updated.Columns.SelectMany(column => column.Cards).Single(card => card.Id == first.Id);
ExpectEqual(string.Empty, updatedCard.Description, "explicit null clears description");
ExpectEqual(null, updatedCard.AssigneeId, "explicit null clears assignee");
ExpectEqual(null, updatedCard.DueDate, "explicit null clears due date");
ExpectEqual(0, updatedCard.Labels.Count, "explicit null clears labels");
ExpectEqual("Low", updatedCard.Priority.ToString(), "priority string is parsed");

```
