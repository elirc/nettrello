# Trace Tables

## Trace: UI To API Move Card

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `client/src/components/KanbanCard.tsx:15-18` | `{ cardId }` JSON string | Card UI | Drag event stores id | Browser DnD support |
| 2 | `client/src/components/KanbanColumn.tsx:28-33` | cardId, columnId, position | Column UI | Parses payload | Invalid payload ignored |
| 3 | `client/src/hooks/useBoard.ts:139-140` | mutation args + version | Hook | Adds board id and version | Stale closure |
| 4 | `client/src/api/boardClient.ts:94-102` | JSON body + header | API client | PUT request | Contract drift |
| 5 | `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:37-38` | route ids + body | Endpoint | Model binding | Missing auth |
| 6 | `src/NetTrello.Api/Application/BoardService.cs:82-88` | domain mutation | Service | Delegates under version check | Conflict |
| 7 | `src/NetTrello.Api/Domain/Board.cs:84-92` | board aggregate | Domain | Move card | Same-column edge |

## Trace: Persistence Update

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `src/NetTrello.Api/Application/BoardService.cs:108-116` | `Func<Board, Board>` | Service | Supplies mutation callback | Rules split |
| 2 | `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:69-75` | board id | Repo | Locks and finds board | Lock contention |
| 3 | `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:77-79` | cloned board | Repo | Updates clone, stores clone, returns clone | Allocations |

## Trace: Error Contract

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `src/NetTrello.Api/Domain/BoardColumn.cs:82-88` | DomainException | Domain | Card limit conflict | Message string coupling |
| 2 | `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:27-30` | exception | Middleware | Problem details | Order sensitive |
| 3 | `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:48-59` | JSON | Middleware | Adds code and trace id | No field errors |
| 4 | `client/src/api/boardClient.ts:39-48` | ApiError | Client | Parses problem | Non-JSON fallback |
| 5 | `client/src/hooks/useBoard.ts:98-103` | UI error | Hook | Sets error and refreshes conflicts | User intent lost |

## Trace: Auth Boundary Gap

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41` | request | Endpoint | Maps routes | No authorization |
| 2 | `src/NetTrello.Api/Domain/Board.cs:113-123` | assignee id | Domain | Validates display member | Not caller identity |
| 3 | `src/NetTrello.Api/Program.cs:19-28` | origin | Host | CORS browser policy | Not access control |

## Trace: Client Initial Load

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `client/src/hooks/useBoard.ts:19-49` | summaries | Hook | Loads list and selects first | Stale fetch |
| 2 | `client/src/hooks/useBoard.ts:51-83` | board id | Hook | Loads detail | Selection race |
| 3 | `client/src/App.tsx:23-44` | render state | App | Shows loading, board, or empty state | UX flicker |

## Trace: Contract Shape

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `src/NetTrello.Api/Contracts/BoardDtos.cs:7-28` | response records | API contract | Defines server JSON shape | DTO changes are breaking |
| 2 | `src/NetTrello.Api/Contracts/BoardMapper.cs:17-54` | domain to DTO | Mapper | Adds assignee name | Mapper cost |
| 3 | `client/src/types.ts:11-40` | TS interfaces | Client | Mirrors JSON | Manual drift |

