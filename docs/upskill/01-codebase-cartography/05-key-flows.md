# Key Flows

## Flow: Initial Board Load

**Why it matters:** This teaches app boot, fetch timing, error state, and how the browser picks a default board.
**Open first:** `client/src/hooks/useBoard.ts:19-49` - initial summaries load.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | UI shell | `client/src/App.tsx:7-24` | `useBoard` feeds sidebar and board area | hook state | Empty board branch must be stable |
| 2 | Hook | `client/src/hooks/useBoard.ts:19-49` | Fetches summaries and selects first board | `BoardSummary[]` | Race handled by `isCurrent` flag |
| 3 | API client | `client/src/api/boardClient.ts:53-56` | Calls `/api/boards` | HTTP GET | Base URL mismatch |
| 4 | Endpoint | `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:13-14` | Delegates list to service | no body | Missing auth means all boards are public |
| 5 | Service | `src/NetTrello.Api/Application/BoardService.cs:17-24` | Sorts and maps summaries | DTO array | Mapping can hide needed fields |
| 6 | Repository | `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:35-40` | Returns cloned boards | domain clones | In-memory only |

**Validation & authorization:** No auth policy found on endpoints `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41`.
**Persistence & side effects:** In-memory read clone at `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:35-40`.
**Tests:** No direct coverage found; no test project in inventory.
**Juniors miss:** Loading summaries and loading detail are separate effects. **Seniors notice:** This prevents the sidebar from depending on full board payloads.
**Drill:** Add a trace log on paper for `selectedBoardId`. **Self-grade:** Basic follows steps; Solid explains `isCurrent`; Strong predicts stale fetch behavior.

## Flow: Create Board

**Why it matters:** Board creation crosses form state, HTTP contract, default domain setup, and repository insertion.
**Open first:** `client/src/components/BoardSidebar.tsx:15-24` - form submit.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | Sidebar | `client/src/components/BoardSidebar.tsx:15-24` | Validates non-empty name client-side | `{ name, description }` | Client validation is not security |
| 2 | Hook | `client/src/hooks/useBoard.ts:115-128` | Calls create, selects result, refreshes summaries | `BoardDetail` | Partial failure after create before refresh |
| 3 | API client | `client/src/api/boardClient.ts:58-62` | POSTs JSON | `CreateBoardInput` | Contract drift |
| 4 | Endpoint | `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:16-20` | Returns `201 Created` | `BoardDetailResponse` | Location path must match id |
| 5 | Service | `src/NetTrello.Api/Application/BoardService.cs:32-43` | Creates board and four default columns | domain board | Defaults are product policy |
| 6 | Domain | `src/NetTrello.Api/Domain/Board.cs:44-55` | Adds columns and bumps version | board aggregate | Version increments multiple times |
| 7 | Repo | `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:49-65` | Adds clone under write lock | clone | No durable persistence |

**Validation & authorization:** Domain validates board name in `src/NetTrello.Api/Domain/Board.cs:8-18`; no authorization layer found.
**Persistence & side effects:** In-memory add under `SemaphoreSlim` at `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:49-65`.
**Tests:** No direct coverage found.
**Drill:** Explain whether default columns belong in service or domain. **Self-grade:** Strong answer mentions product policy vs invariant.

## Flow: Add Card

**Why it matters:** This flow shows form input normalization, enum/string contracts, board version headers, and assignee validation.
**Open first:** `client/src/components/CardComposer.tsx:18-35`.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | Composer | `client/src/components/CardComposer.tsx:18-35` | Builds `CardInput`, trims labels | client input | No description/due date here |
| 2 | Column | `client/src/components/KanbanColumn.tsx:54-54` | Binds card to current column | `column.id` | Wrong column id misroutes |
| 3 | Hook | `client/src/hooks/useBoard.ts:133-134` | Sends current board version | `CardInput` | Stale board closure |
| 4 | API client | `client/src/api/boardClient.ts:74-82` | POST with `X-Board-Version` | JSON | Missing header disables conflict check |
| 5 | Endpoint | `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:31-32` | Reads expected version | DTO | Header parse can fail |
| 6 | Service | `src/NetTrello.Api/Application/BoardService.cs:64-71` | Delegates add card mutation | domain args | Use case owns orchestration |
| 7 | Domain | `src/NetTrello.Api/Domain/Board.cs:66-73` | Checks assignee and adds card | `Card` | Member list is currently seeded only |
| 8 | Column | `src/NetTrello.Api/Domain/BoardColumn.cs:43-48` | Inserts and reindexes | ordered cards | Limit conflict after insert path |

**Validation & authorization:** Assignee must exist at `src/NetTrello.Api/Domain/Board.cs:113-123`; no auth boundary found.
**Persistence & side effects:** Stored through repository update at `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:67-85`.
**Tests:** No direct coverage found.
**Drill:** Find every line that can reject a card. **Self-grade:** Strong includes client, endpoint header, service conflict, domain validation, card limit.

## Flow: Move Card With Optimistic Concurrency

**Why it matters:** This is the most important production idea in the repo: prevent stale browser state from silently overwriting another change.
**Open first:** `src/NetTrello.Api/Application/BoardService.cs:106-119`.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | Card UI | `client/src/components/KanbanCard.tsx:15-27` | Drag payload stores `cardId` and drop stops bubbling | JSON string | Browser DnD quirks |
| 2 | Column UI | `client/src/components/KanbanColumn.tsx:28-33` | Reads dragged card and target position | ids and position | Invalid payload ignored |
| 3 | Hook | `client/src/hooks/useBoard.ts:139-143` | Calls move/archive with current version | mutation | Stale closure possible under rapid actions |
| 4 | API client | `client/src/api/boardClient.ts:94-102` | PUTs target column and position | `MoveCardRequest` | Contract drift |
| 5 | Endpoint | `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:37-38` | Reads version header | route + body | Missing header allows last write wins |
| 6 | Service | `src/NetTrello.Api/Application/BoardService.cs:106-119` | Rejects stale version | conflict | User must refresh/retry |
| 7 | Domain | `src/NetTrello.Api/Domain/Board.cs:84-92` | Removes card, inserts into target, touches board | board aggregate | Same-column move edge cases |
| 8 | Hook recovery | `client/src/hooks/useBoard.ts:98-103` | On `409`, refetches board | `BoardDetail` | User intent is lost unless retried |

**Validation & authorization:** Version header is optional at `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:46-59`; no user permission checks found.
**Persistence & side effects:** In-memory update locks writes at `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:67-85`.
**Tests:** No direct coverage found.
**Drill:** Write a two-tab conflict scenario. **Self-grade:** Strong proposes UX copy and a regression test.

## Flow: Edit And Archive Card

**Why it matters:** Shows local form state, nullable fields, date conversion, update vs delete semantics.
**Open first:** `client/src/components/CardEditor.tsx:22-40`.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | Editor | `client/src/components/CardEditor.tsx:14-20` | Initializes local form from card | local state | Props change while modal open |
| 2 | Editor | `client/src/components/CardEditor.tsx:22-35` | Converts form to `CardInput` | title, description, date, labels | Timezone/date conversion |
| 3 | Hook | `client/src/hooks/useBoard.ts:136-143` | Sends update or archive | mutation | No optimistic UI |
| 4 | API client | `client/src/api/boardClient.ts:84-111` | PATCH or DELETE | HTTP | Archive returns whole board |
| 5 | Endpoint | `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:34-41` | Calls service | route ids | No per-card auth |
| 6 | Domain | `src/NetTrello.Api/Domain/Card.cs:48-62` | Updates fields | card | Labels normalized in domain |
| 7 | Domain | `src/NetTrello.Api/Domain/Board.cs:95-99` | Archive removes active card | board | No restore/audit |

**Tests:** No direct coverage found.
**Drill:** Decide whether "archive" should be DELETE or PATCH to `archived=true`. **Self-grade:** Strong discusses audit, idempotency, and API compatibility.

## Flow: Error Handling

**Why it matters:** Production systems need stable failure shapes so clients can respond without parsing random strings.
**Open first:** `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:21-60`.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | Domain | `src/NetTrello.Api/Domain/DomainException.cs:3-20` | Carries code, message, status | exception | Domain knows HTTP status numbers |
| 2 | Middleware | `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:27-40` | Converts exceptions to problem details | RFC 7807-ish JSON | Middleware order matters |
| 3 | Problem body | `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:43-60` | Adds `code` and `traceId` | JSON | Logs only unhandled errors |
| 4 | Client | `client/src/api/boardClient.ts:39-48` | Parses problem and throws `ApiError` | typed error | Non-JSON error fallback |
| 5 | Hook | `client/src/hooks/useBoard.ts:98-103` | Handles conflict specially | UI error | No per-field validation display |

**Tests:** No direct coverage found.
**Drill:** Sketch a validation error UI that uses `code`. **Self-grade:** Strong preserves server as source of truth.

## Flow: Security Headers And CORS

**Why it matters:** Cross-cutting platform behavior belongs near the host, not scattered through features.
**Open first:** `src/NetTrello.Api/Program.cs:19-34`.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | Config | `src/NetTrello.Api/appsettings.json:1-8` | Lists allowed origins | strings | Environment mismatch |
| 2 | Host | `src/NetTrello.Api/Program.cs:19-28` | Registers CORS policy | options | `AllowAnyHeader/Method` is broad |
| 3 | Middleware | `src/NetTrello.Api/Program.cs:32-34` | Exception, security headers, CORS order | HTTP pipeline | Order can change behavior |
| 4 | Headers | `src/NetTrello.Api/Middleware/SecurityHeadersMiddleware.cs:12-20` | Adds browser hardening headers | response headers | No CSP yet |

**Validation & authorization:** This is not authorization. No `[Authorize]`, policy, or identity code was found.
**Tests:** No direct coverage found.
**Drill:** Explain why CORS is not access control. **Self-grade:** Strong mentions browsers, non-browser clients, and server-side authorization.

