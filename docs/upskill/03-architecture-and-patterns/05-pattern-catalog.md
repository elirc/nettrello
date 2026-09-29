# Pattern Catalog

## Pattern: Composition Root
**Problem:** Keep application wiring visible. **Real example:** `src/NetTrello.Api/Program.cs:7-39`. **Why it works:** DI, middleware, JSON, CORS, and endpoints are discoverable. **Failure modes:** `Program.cs` becomes a dumping ground. **Drill:** Add one service registration on paper and justify lifetime.

## Pattern: Minimal API Route Group
**Problem:** Keep related routes together. **Real example:** `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:9-60`. **Why it works:** One board surface, shared tag, consistent version header. **Failure modes:** Business logic in lambdas. **Drill:** Sketch `rename column` endpoint.

## Pattern: Application Service
**Problem:** Orchestrate use cases outside HTTP and domain. **Real example:** `src/NetTrello.Api/Application/BoardService.cs:17-119`. **Why it works:** Endpoints stay thin; domain stays focused. **Failure modes:** Service becomes transaction script with duplicated rules. **Drill:** Identify one policy in service vs one invariant in domain.

## Pattern: Repository Abstraction
**Problem:** Hide storage mechanism. **Real example:** `src/NetTrello.Api/Application/IBoardRepository.cs:5-15`; implementation `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:7-85`. **Why it works:** EF Core can replace memory later. **Failure modes:** Leaky query abstractions. **Drill:** Add a method only if a use case needs it.

## Pattern: Aggregate Invariants
**Problem:** Keep state valid after every mutation. **Real example:** `src/NetTrello.Api/Domain/Board.cs:44-99`. **Why it works:** Column/card operations touch version and reindex. **Failure modes:** Public setters bypass rules. **Drill:** Prove `MoveCard` preserves one card instance.

## Pattern: Guard Helpers
**Problem:** Centralize common validation. **Real example:** `src/NetTrello.Api/Domain/Guard.cs:3-34`. **Why it works:** Consistent required/max/non-negative behavior. **Failure modes:** Guards grow into business rules. **Drill:** Decide whether email format belongs here.

## Pattern: DTO Mapping
**Problem:** Separate HTTP shape from domain. **Real example:** `src/NetTrello.Api/Contracts/BoardMapper.cs:7-54`. **Why it works:** Client can receive assignee name without mutating domain. **Failure modes:** Mapping gets expensive or duplicated. **Drill:** Add `overdue` as computed DTO only.

## Pattern: Optimistic Concurrency
**Problem:** Prevent stale writes. **Real example:** `src/NetTrello.Api/Application/BoardService.cs:106-119`; client header `client/src/api/boardClient.ts:30-32`. **Why it works:** Server rejects outdated versions. **Failure modes:** Optional header allows last-write-wins. **Drill:** Make header required for mutating routes.

## Pattern: Problem Details
**Problem:** Stable error contract. **Real example:** `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:43-60`; client parser `client/src/api/boardClient.ts:39-48`. **Why it works:** UI can handle `code` and status. **Failure modes:** Sensitive details in development leak if environment mis-set. **Drill:** Add field errors.

## Pattern: Security Headers Middleware
**Problem:** Cross-cutting HTTP hardening. **Real example:** `src/NetTrello.Api/Middleware/SecurityHeadersMiddleware.cs:12-20`. **Why it works:** Central response policy. **Failure modes:** Missing CSP, duplicate headers through proxies. **Drill:** Propose CSP for the API.

## Pattern: Typed API Client
**Problem:** Avoid scattering fetch calls. **Real example:** `client/src/api/boardClient.ts:24-112`. **Why it works:** Base URL, headers, and errors are central. **Failure modes:** Types drift from backend. **Drill:** Add generated OpenAPI later.

## Pattern: Hook As Orchestrator
**Problem:** Keep components render-focused. **Real example:** `client/src/hooks/useBoard.ts:5-159`. **Why it works:** Loading, saving, errors, and actions are in one place. **Failure modes:** Hook grows into a store. **Drill:** Split read and mutation hooks only when complexity demands it.

## Pattern: Effect Cleanup Flag
**Problem:** Ignore stale async responses after unmount or selection change. **Real example:** `client/src/hooks/useBoard.ts:19-49`, `client/src/hooks/useBoard.ts:51-83`. **Why it works:** Prevents late state updates. **Failure modes:** Does not cancel network request. **Drill:** Replace with `AbortController`.

## Pattern: Local Form State
**Problem:** Let user edit fields before saving. **Real example:** `client/src/components/CardEditor.tsx:14-35`. **Why it works:** Modal can stage changes. **Failure modes:** Prop changes can stale local state. **Drill:** Add `key={selectedCard.id}` to modal owner and explain.

## Pattern: Browser Drag Payload
**Problem:** Move cards without global drag state. **Real example:** `client/src/components/KanbanCard.tsx:15-27`, `client/src/components/KanbanColumn.tsx:15-33`. **Why it works:** DataTransfer carries card id. **Failure modes:** Payload parsing fails silently. **Drill:** Add keyboard move controls.

## Pattern: Clone For Isolation
**Problem:** Avoid accidental mutation of stored aggregate. **Real example:** `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:38-45`, `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:77-79`. **Why it works:** Callers cannot hold stored references. **Failure modes:** Allocations scale with board size. **Drill:** Compare with immutable records.

## Pattern: Build Strictness
**Problem:** Catch defects early. **Real example:** `src/NetTrello.Api/NetTrello.Api.csproj:3-7`, `client/tsconfig.json:10-17`. **Why it works:** Null/type errors block builds. **Failure modes:** Too many style errors slow learning. **Drill:** Explain one warning you would keep as error.

