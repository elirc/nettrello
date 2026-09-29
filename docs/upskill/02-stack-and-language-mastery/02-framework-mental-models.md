# Framework Mental Models

## ASP.NET Core Pipeline

The request pipeline is ordered in `src/NetTrello.Api/Program.cs:30-39`: build app, exception middleware, security headers, CORS, health checks, board endpoints, run. Middleware order matters because exception middleware at `src/NetTrello.Api/Program.cs:32-32` wraps later middleware and endpoints; security headers at `src/NetTrello.Api/Program.cs:33-33` adds response headers before CORS and endpoint responses complete.

Failure mode: placing exception middleware after endpoints would miss endpoint exceptions.

Drill: Draw a request to `PUT /api/boards/{id}/cards/{id}/move` through each pipeline line.

## Dependency Injection Lifetimes

`IClock` and `IBoardRepository` are singletons, while `BoardService` is scoped in `src/NetTrello.Api/Program.cs:11-13`. This is safe because scoped services can depend on singleton services. The captive dependency bug is the inverse: a singleton depending on a scoped service, causing request-specific state to be held forever.

Drill: If the repository later depends on EF Core `DbContext`, should it remain singleton? Answer: no; `DbContext` should be scoped per request.

## Minimal API Model Binding

Routes bind `Guid` route values, JSON bodies, `HttpRequest`, services, and cancellation tokens directly in endpoint lambdas (`src/NetTrello.Api/Endpoints/BoardEndpoints.cs:22-41`). Request DTO records are declared in `src/NetTrello.Api/Contracts/BoardDtos.cs:32-54`.

Failure mode: route/body names drift from client paths in `client/src/api/boardClient.ts:64-111`.

## React Render Model

`App` renders from hook state in `client/src/App.tsx:7-49`. State updates in `useBoard` (`client/src/hooks/useBoard.ts:6-11`) trigger re-renders. Effects load data after render and clean up stale async responses with `isCurrent` flags (`client/src/hooks/useBoard.ts:19-49`, `client/src/hooks/useBoard.ts:51-83`).

Failure modes:

- Stale closures: `mutateBoard` captures `board` and `refreshBoards` in `client/src/hooks/useBoard.ts:85-109`.
- Prop-derived local state: `CardEditor` copies props to state in `client/src/components/CardEditor.tsx:14-20`; if the selected card changes without remounting, state may be stale.

Drill: Explain why `key={column.id}` in `client/src/components/KanbanBoard.tsx:49-57` helps React preserve column identity.

