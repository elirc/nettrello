# Practice exercises

Use the seeded board id from `InMemoryBoardRepository` only for HTTP exercises; use synthetic IDs in new domain fixtures. First, list the call graph from PATCH route to `Card.Update` and mark where the current card is read (`BoardService.cs:77-79`).

Every exercise has a **Goal** and a **Check**. A Check either names the existing test that encodes the answer — run one with
`dotnet test tests/NetTrello.Tests/NetTrello.Tests.csproj -c Release --filter "FullyQualifiedName~<TestName>"` — or states what your new test must assert. Write an expected-state table (response, writes, version, timestamps, visible fields) before running anything.

**1. Omitted versus null.**
- Goal: send `{"title":"Renamed"}` to a card whose description, priority, assignee, due date and labels are populated; predict every field. Then send `{"description":null,"labels":null}` and explain why both clear.
- **Check:** `CardPatchTests.TitleOnlyPatch_AppliesTitleAndPreservesEveryOmittedField` and `CardPatchTests.ExplicitNulls_ClearNullableFields_AndAStringPriorityIsParsed`; over HTTP, `BoardApiTests.TitleOnlyPatch_Returns200_AndPreservesTheOmittedDescription`.

**2. Malformed bodies.**
- Goal: for `{}`, `{"priority":"Imaginary"}`, `{"title":null}` and `{"labels":["ok",4]}`, predict status, problem code and whether the board version changes. Use a table `body | status | code | version delta | changed fields`.
- **Check:** the five rows of `CardPatchTests.MalformedPatch_IsRejectedWithItsContractCode_AndChangesNothing` (service level, asserts an unchanged reloaded board) and the two rows of `BoardApiTests.MalformedPatch_Returns400_WithItsContractCode` (HTTP level).

**3. Rejected moves.**
- Goal: with the full "Target" column from `BoardFixture`, attempt a cross-column move, a move to a missing column and a move to position -1. Record source order, target order, card position, timestamp and version before and after each rejection.
- **Check:** `BoardAggregateTests.MoveCard_ToAFullColumn_...`, `MoveCard_ToAMissingColumn_...` and `MoveCard_ToANegativePosition_...`; each compares whole-board snapshots including `Version` and `UpdatedAt`. Then name the order of the checks in `Board.cs:86-107`: target lookup (line 89), position (90), same-column branch (92-97), capacity (99-102), and only then removal (104).

**4. Reorder inside a full column.**
- Goal: explain why capacity is not a reason to reject a same-column move and how positions are reindexed.
- **Check:** `BoardAggregateTests.MoveCard_ReordersWithinItsOwnColumn_EvenWhenAnotherColumnIsFull` covers the unlimited source column only. Write the missing case: make a column full, reorder within it, and assert success plus contiguous positions `0..n-1` (`BoardColumn.RepositionCard`, lines 50-58).

**5. Two writers, one version.**
- Goal: two service mutations carry the same `X-Board-Version`; design assertions proving exactly one success, one conflict and no lost change. Name the lock and the clone separately.
- **Check:** `BoardAggregateTests.TwoWritesWithTheSameExpectedVersion_LeaveExactlyOneWinner` asserts the 1/1 split. Extend it to assert the final board has exactly one new column and `Version` increased by exactly one.

**6. Clone isolation.**
- Goal: mutate a board returned by `GetAsync`, read again, and name the repository property that keeps the second read unchanged.
- **Check:** `BoardAggregateTests.TheRepositoryHandsOutAnIsolatedClone`; the property is the `Clone()` on read (`InMemoryBoardRepository.cs:45`) and on store (line 78). Mutating the returned object is not a persistence operation; only `UpdateAsync` is.

**7. Which layer owns which failure.**
- Goal: assign each failure above to domain, service, repository or HTTP tests, without claiming an HTTP status from a pure domain assertion.
- **Check:** your table matches the three classes in chapter 04's table; every HTTP status you cite comes from a `BoardApiTests` case.

**8. Write the positive move test yourself.**
- Goal: in `BoardAggregateTests.cs`, remove one card from the full target column, then move a source card to a valid target position. Assert source column, target column, the moved card's `Position`, and that `board.Version` increased by exactly one.
- **Check:** the new test passes; and replacing the four mutating lines of `Board.MoveCard` (`Board.cs:104-107`) with `throw DomainException.Conflict("column.card_limit", "x");` makes it fail while the three rejection tests and the same-column reorder test stay green. That is the proof that a suite with only rejection tests can be passed by an implementation that rejects everything.

**9. Add a row to the theory.**
- Goal: add `{"assigneeId":"not-a-guid"}` to `MalformedPatch_IsRejectedWithItsContractCode_AndChangesNothing` and predict the code first.
- **Check:** the row passes with `card.field_invalid`, raised by `ReadNullableGuid` in `BoardDtos.cs` ("assigneeId must be a GUID or null."). Decide whether a field-generic code is a reporting gap; compare with the dedicated `card.priority_invalid` and `card.labels_invalid` codes.

## Lab: red-first from the snapshots

The exact pre-change files are at the project root under `../snapshots/`. **Do not copy whole snapshot files over the sources:** the current `BoardService.cs:77-78` calls `board.GetCard` and `request.ResolveAgainst`, which do not exist in `Board-before.cs.txt` and `BoardDtos-before.cs.txt`, and `CardPatchTests.cs:55` constructs the new `UpdateCardRequest` with property initializers. A whole-file copy of either fails to compile (CS1061 for the missing methods; the positional record also has no parameterless constructor for the test's initializer), so no test runs. Revert behavior, not files:

1. **Card.** Copy `../snapshots/Card-before.cs.txt` over `src/NetTrello.Api/Domain/Card.cs`. This one compiles, because nothing outside `Card` calls the new private `EnsurePriority`.
2. **Board.MoveCard.** In `src/NetTrello.Api/Domain/Board.cs`, replace only the body of `MoveCard` with the snapshot's body (`Board-before.cs.txt:86-92`: remove from source, `MoveTo`, find target, `AddCard`, `Touch`). Keep `GetCard`.
3. **Predict in writing** which of the 21 tests go red and on which assertion, then run `dotnet test tests/NetTrello.Tests/NetTrello.Tests.csproj -c Release`.
4. Restore with `git checkout -- src` and re-run to 21 green. Write one sentence per failure naming the line of the repair that fixed it.

**Check (derived from the source on 2026-10-06, not from a recorded run; confirm with yours):** 4 red, 17 green.
- `CardUpdate_WithABlankLabel_PreservesEveryField`: the error code still matches (`validation.required`), but the card snapshot differs because the old `Update` assigned title, description and priority before `NormalizeLabels` threw.
- `MoveCard_ToAMissingColumn_...`, `MoveCard_ToAFullColumn_...`, `MoveCard_ToANegativePosition_...`: each still throws the expected code, but the whole-board snapshot differs because the card was already removed from its source.
- Green, and worth explaining: `MoveCard_ReordersWithinItsOwnColumn_...` (the source column is unlimited, so remove-then-add still yields `Second,First`), every PATCH test (the priority and label checks the old `Card` lacked are also enforced by `ResolveAgainst`), and the version and clone tests.

The red tests fail on the *second* assertion, not the first: the exception code alone cannot see a partial mutation. That is the case for snapshot assertions.
