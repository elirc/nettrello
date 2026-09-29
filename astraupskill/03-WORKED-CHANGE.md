# Worked change: presence-aware card PATCH

Before the repair, the route bound `UpdateCardRequest` with required CLR properties and the service passed every property to `Card.Update`. A title-only body therefore became a title plus null description, default priority, null assignee, null due date, and empty labels. The new request keeps raw JSON presence and resolves against the current card:

```csharp
var card = board.GetCard(cardId);
var values = request.ResolveAgainst(card);
board.UpdateCard(cardId, values.Title, values.Description, values.Priority,
    values.AssigneeId, values.DueDate, values.Labels, _clock.UtcNow);
```

`ResolveAgainst` rejects `{}` with `card.patch_empty`, rejects a null or non-string title, parses only defined enum values, and treats explicit null as a clear for nullable properties. It returns the old value for omitted properties. The existing React editor sends all six fields, so its complete update remains compatible. This is an actual placement excerpt from `BoardService.UpdateCardAsync`; the complete parser is in `Contracts/BoardDtos.cs`.

The second change repairs domain ordering. The old move removed the card before finding the destination and before validating position. The new `Board.MoveCard` finds source and target, validates position, handles same-column reorder, checks target capacity, and only then removes and inserts. `Card.Update` calculates normalized values first. `BoardColumn.AddCard` checks capacity before insertion. These changes preserve board version, card fields, order, and timestamps on rejected operations. `BoardAggregateTests` in [the xUnit suite](../tests/NetTrello.Tests) captures a whole-board snapshot before each rejected operation — including `Version` and `UpdatedAt` — and asserts it is byte-identical afterwards.


The presence parser deliberately keeps transport concerns in the contracts layer. The domain still receives ordinary typed values and does not know what JSON syntax produced them. That keeps `Card.Update` usable from other application code while making HTTP PATCH precise. Explicit null labels become an empty list after normalization; explicit null description becomes the domain's empty description because `Guard.Optional` defines that representation. If a future API needs to distinguish null from empty internally, that is a separate domain decision and should be represented by a new type, not inferred from a missing JSON property.


The same approach applies if a future field is added. Add a nonnullable `JsonElement` property, define its omitted/null/value cases, include it in the empty-patch test, and add one member to `CardUpdateValues`. Then make the domain validate the resulting complete value before assigning. This sequence avoids the common mistake of adding a nullable DTO property and accidentally turning omission into a clear. It also gives the API a single place to document malformed types and status codes.
