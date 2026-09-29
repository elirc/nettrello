# Validation, Auth, And Permissions

## Validation Layers Present

| Layer | Evidence | What it catches |
| --- | --- | --- |
| Client forms | `client/src/components/BoardSidebar.tsx:15-24`, `client/src/components/CardComposer.tsx:18-35` | Empty user input before sending |
| Endpoint header parsing | `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:46-59` | Invalid `X-Board-Version` |
| Domain guard | `src/NetTrello.Api/Domain/Guard.cs:3-34` | Required strings, max lengths, non-negative numbers |
| Domain aggregate | `src/NetTrello.Api/Domain/Board.cs:113-123` | Assignee must belong to board |
| Column invariant | `src/NetTrello.Api/Domain/BoardColumn.cs:82-88` | Card limit |

Client validation improves UX; domain validation protects the system.

## Auth Boundary

No authentication or authorization code was found. Endpoints in `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41` do not call `RequireAuthorization`, use `[Authorize]`, or check user identity. Members currently exist as display data in seed storage (`src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:15-22`), not as security principals.

## Junior Misses vs Senior Checks

- Junior miss: "CORS protects the API." Senior check: CORS is configured in `src/NetTrello.Api/Program.cs:19-28`, but non-browser clients can still call endpoints.
- Junior miss: "Assignee check is authorization." Senior check: `EnsureAssigneeExists` only validates display membership at `src/NetTrello.Api/Domain/Board.cs:113-123`; it does not prove the caller can edit the board.
- Junior miss: "Hidden UI button is permission." Senior check: server endpoints must enforce resource access.

## Drill

Design board membership authorization for `MoveCardAsync`. Anchor your answer to endpoint route data at `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:37-38` and service mutation at `src/NetTrello.Api/Application/BoardService.cs:82-88`.

