# Code Review Mindset

Review in this order:

1. Does it work? Trace endpoint to domain, such as `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:37-38` to `src/NetTrello.Api/Domain/Board.cs:84-92`.
2. Is it correct? Check invariants like card limits in `src/NetTrello.Api/Domain/BoardColumn.cs:82-88`.
3. Will it stay correct? Ask for tests around service conflict logic in `src/NetTrello.Api/Application/BoardService.cs:106-119`.
4. Does it fit? Keep HTTP in endpoints, orchestration in service, invariants in domain, UI state in hooks.
5. Is it kind to maintainers? Avoid broad refactors unrelated to the ticket.

Good comment example:

> The UI change looks right, but this mutation bypasses the board version pattern used in `client/src/api/boardClient.ts:30-32` and `src/NetTrello.Api/Application/BoardService.cs:106-119`. Please send the version header and add a conflict test.

