# Testing Strategy

## Test Layers

| Layer | What belongs here | Anchors |
| --- | --- | --- |
| Domain unit tests | Guard validation, card movement, limits, version increments | `src/NetTrello.Api/Domain/Board.cs:44-156`, `src/NetTrello.Api/Domain/BoardColumn.cs:43-88`, `src/NetTrello.Api/Domain/Card.cs:48-82` |
| Application tests | Conflict behavior and use-case sequencing | `src/NetTrello.Api/Application/BoardService.cs:46-119` |
| Repository contract tests | Add/get/update clone isolation | `src/NetTrello.Api/Application/IBoardRepository.cs:5-15`, `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:35-85` |
| API integration tests | Routes, problem details, headers, CORS | `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-60`, `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:21-60` |
| Client component tests | Forms, drag/drop events, error banner | `client/src/components/CardComposer.tsx:18-75`, `client/src/components/KanbanCard.tsx:10-59`, `client/src/App.tsx:14-21` |
| E2E tests | Create board, add card, move card, conflict message | Crosses `client/src/hooks/useBoard.ts:115-147` and API endpoints |

## What Not To Test

- Do not snapshot every CSS class in `client/src/styles/app.css:1-342`.
- Do not unit-test C# record property assignment in `src/NetTrello.Api/Contracts/BoardDtos.cs:5-54`.
- Do not mock the domain when testing `BoardService`; the domain rules are the point.

## Flake Prevention

Use `IClock` from `src/NetTrello.Api/Application/IClock.cs:1-6` to inject deterministic time in tests. Avoid depending on `DateTimeOffset.UtcNow` seed values in `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:14-14`.

