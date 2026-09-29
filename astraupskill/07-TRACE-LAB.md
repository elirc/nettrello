# Trace lab

Trace A follows `PATCH /api/boards/{id}/cards/{id}`. Minimal API binding creates `UpdateCardRequest` with six nonnullable `JsonElement` properties. The service callback runs under the repository lock, gets the cloned board, finds the card, and calls `ResolveAgainst`. For `{"title":"HTTP title only"}`, title is replaced while description and other omitted fields come from the card. `Board.UpdateCard` verifies the assignee and `Card.Update` validates title, description, and labels before assignment. `Touch` increments version and the mapper serializes the response.

Trace B follows a failed move. `Board.MoveCard` finds source and destination. If destination is absent, `FindColumn` throws `column.not_found`; if it is full, the explicit capacity check throws `column.card_limit`; if position is negative, `Guard.NonNegative` throws. Source removal is below all three checks. Because repository `UpdateAsync` was working on a clone, stored state remains intact even if a future branch mutates before throwing.

Trace C follows competing writes. Request one and request two both carry version 5. The semaphore serializes callbacks. Request one changes the aggregate to version 6. Request two then sees 6 and throws 409. The middleware writes a ProblemDetails object with `status`, `title`, `detail`, `instance`, `code`, and `traceId`. Record actor/input/current version/decision/stored version/HTTP response in a trace ledger. This separates a stale-client conflict from a domain validation error and from a repository clone defect.


For a second pass, add a trace row for explicit null description. The parser sees `JsonValueKind.Null`, resolves null, `Guard.Optional` stores an empty string, and the mapper emits `description: ""`. Add another row for omitted description; it returns the prior description. These rows look similar in the UI but have different transport meanings. A trace row for `labels: []` is also distinct from omitted labels: empty array intentionally replaces the old labels, while omission preserves them. This is why a raw DTO with nullable CLR properties was insufficient.


When a trace ends in a conflict, no mapper response is produced by the service callback; the middleware receives the exception and emits a problem document. When a trace ends in a successful mutation, the repository returns a clone and the service maps that clone. The distinction matters because returning the callback's working object would expose a different lifetime and could let a caller retain mutable state. The current repository intentionally clones both paths.

## Why omitted and explicit-null differ

Trace A and the second-pass rows both depend on `ResolveAgainst`, which reads `JsonValueKind` rather than a nullable CLR property: `Undefined` means "keep the stored value", and any other kind means the caller sent something. From `src/NetTrello.Api/Contracts/BoardDtos.cs:63-78`:

```csharp
    public CardUpdateValues ResolveAgainst(Card current)
    {
        if (Title.ValueKind == JsonValueKind.Undefined && Description.ValueKind == JsonValueKind.Undefined && Priority.ValueKind == JsonValueKind.Undefined && AssigneeId.ValueKind == JsonValueKind.Undefined && DueDate.ValueKind == JsonValueKind.Undefined && Labels.ValueKind == JsonValueKind.Undefined)
        {
            throw DomainException.BadRequest("card.patch_empty", "At least one card field is required.");
        }

        return new CardUpdateValues(
            ReadRequiredString(Title, current.Title, "title"),
            ReadNullableString(Description, current.Description, "description"),
            ReadPriority(Priority, current.Priority),
            ReadNullableGuid(AssigneeId, current.AssigneeId, "assigneeId"),
            ReadNullableDate(DueDate, current.DueDate, "dueDate"),
            ReadNullableLabels(Labels, current.Labels));
    }
```

