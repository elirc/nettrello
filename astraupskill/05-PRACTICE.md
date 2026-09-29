# Practice exercises

Use the seeded board id from `InMemoryBoardRepository` only for HTTP exercises; use synthetic IDs in new domain fixtures. First, list the call graph from PATCH route to `Card.Update` and mark where the current card is read.

1. Send `{"title":"Renamed"}` to a card whose description, priority, assignee, due date, and labels are populated. Predict every field after the response. Then send `{"description":null,"labels":null}` and explain why both clear.
2. Send `{}`, `{"priority":"Imaginary"}`, `{"title":null}`, and `{"labels":["ok",4]}`. For each, predict status, problem code, and whether board version changes.
3. Create a full target column at its limit. Attempt a cross-column move, a missing target move, and a negative position. Record source order, target order, card position, timestamp, and version before and after each rejection.
4. Move two cards within the same full column. Explain why capacity is not a reason to reject and how positions should be reindexed.
5. Start two service mutations with the same `X-Board-Version`. Design assertions proving exactly one success, one conflict, and no lost column/card changes.
6. Mutate a board returned by `GetAsync`, then read it again. What repository property must make the second read unchanged?
7. Decide which failures belong to domain tests, service tests, repository tests, and HTTP tests. Do not claim an HTTP status from a pure domain assertion.

For each exercise write an expected-state table with response, writes, version, timestamps, and visible fields. Then compare your answer with the matched solutions chapter.


For exercise 2, include a table like `body | status | code | version delta | changed fields`. For exercise 3, include source and target card id order as well as counts. For exercise 5, record both returned status and final repository version, then explain why either request may win without violating correctness. A strong answer names the lock and the clone separately. For exercise 6, distinguish mutating the returned board object from calling `UpdateAsync`; only the latter is a persistence operation. These tables make the exercise reviewable without relying on an implementation guess.


After writing each prediction, open the corresponding method and identify the first mutating statement. For PATCH it should be after `ResolveAgainst`; for a move it should be after target and capacity checks; for a repository update it should be inside the semaphore. If your expected table cannot name a precise first mutation, the design is still unclear. Extend the harness only with synthetic members and cards, and keep each new scenario isolated from the seeded HTTP process.


## Exercise 8: write the [Fact] yourself

Open `tests/NetTrello.Tests/BoardAggregateTests.cs`. Add a test that moves a card to a *valid* position in the target column after first removing one of the two cards that fill it, and assert the source column, the target column, the moved card's `Position`, and that `board.Version` increased by exactly one. Predict each value before running. This is the positive counterpart to the three rejection tests, and a suite without it can be passed by an implementation that rejects everything.

## Exercise 9: add a row to the theory

Add `{"assigneeId":"not-a-guid"}` to the `[InlineData]` rows of `MalformedPatch_IsRejectedWithItsContractCode_AndChangesNothing`. Predict the contract code before you run it; if the run disagrees with your prediction, decide which one is wrong — the test or the service — and say why. (Hint: read `ResolveAgainst` in `BoardService`; a code you did not expect may be a real reporting gap rather than your mistake.)

## Lab: red-first from the snapshots

The exact pre-change files are at the project root under `../snapshots/`: `Card-before.cs.txt`, `BoardColumn-before.cs.txt`, `Board-before.cs.txt`, `BoardDtos-before.cs.txt`, `BoardService-before.cs.txt`.

1. Copy `Board-before.cs.txt` over `src/NetTrello.Api/Domain/Board.cs` and `BoardDtos-before.cs.txt` over `src/NetTrello.Api/Contracts/BoardDtos.cs`.
2. **Predict, in writing, which of the 21 tests go red.** Be specific about the PATCH ones: with the pre-change DTO, what does a title-only body do to the description, and which assertion catches it?
3. Run `dotnet test tests/NetTrello.Tests/NetTrello.Tests.csproj -c Release` and record every failing name and message.
4. Compare. Pay attention to any test you expected to fail that passed — that is a gap in the suite, and finding one is a better outcome than a clean prediction.
5. Restore the delivered files, re-run to green, and write one sentence per failure naming the line of the repair that fixed it.

This lab is also the fastest way to see why the old fail-fast console harness was replaced: with the pre-change files it stopped at the first mismatch, so you learned about one broken behaviour per run instead of all of them.
