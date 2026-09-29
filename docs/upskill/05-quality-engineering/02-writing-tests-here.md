# Writing Tests Here

No test project exists yet. Commands below are inferred.

## Recipe 1: Domain Happy Path

Command: `dotnet test --filter MoveCard`
Test `src/NetTrello.Api/Domain/Board.cs:84-92`: moving a card changes source/target columns and increments version via `src/NetTrello.Api/Domain/Board.cs:152-156`.

## Recipe 2: Validation Failure

Command: `dotnet test --filter CardTitleRequired`
Test `src/NetTrello.Api/Domain/Card.cs:48-56` and `src/NetTrello.Api/Domain/Guard.cs:3-16`: empty title throws `DomainException`.

## Recipe 3: Permission Failure Placeholder

Command: `dotnet test --filter BoardAuthorization`
Expected to fail until auth exists. Anchor missing auth at `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41`.

## Recipe 4: Conflict

Command: `dotnet test --filter VersionConflict`
Test `src/NetTrello.Api/Application/BoardService.cs:106-119`: stale `expectedVersion` throws conflict and does not update repository.

## Recipe 5: API Problem Details

Command: `dotnet test --filter ProblemDetails`
Use future `WebApplicationFactory<Program>` enabled by partial `Program` at `src/NetTrello.Api/Program.cs:41-43`; assert `code` and `traceId` from `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:56-57`.

## Recipe 6: UI Error State

Command: `npm run test -- CardError`
Future test should assert error banner from `client/src/App.tsx:14-21` when `boardClient` throws `ApiError` from `client/src/api/boardClient.ts:39-48`.

## Recipe 7: Drag/Drop

Command: `npm run test -- MoveCard`
Simulate drag start in `client/src/components/KanbanCard.tsx:15-18` and drop handling in `client/src/components/KanbanColumn.tsx:28-33`.

## Recipe 8: Contract Drift

Command: `dotnet test --filter ContractShape`
Assert server priority strings match `client/src/types.ts:1-1` and converter registration exists at `src/NetTrello.Api/Program.cs:14-17`.

