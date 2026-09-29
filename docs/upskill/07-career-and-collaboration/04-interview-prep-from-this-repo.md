# Interview Prep From This Repo

## C# / .NET Runtime

**Question:** Why is `InMemoryBoardRepository` registered singleton?
Junior answer: It keeps data available while the app runs.
Mid-level adds: It is safe because it does not depend on scoped services and uses a write lock (`src/NetTrello.Api/Program.cs:11-13`, `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:9-10`).
Senior includes: It is not durable, not multi-instance, and would be wrong if it depended on EF `DbContext`.

**Question:** Why pass `CancellationToken`?
Junior: To cancel work.
Mid-level: Endpoints pass it to services and repository (`src/NetTrello.Api/Endpoints/BoardEndpoints.cs:13-41`, `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:35-69`).
Senior: It prevents wasted work and should flow into future DB/outbound calls.

## ASP.NET Core

**Question:** Explain the middleware order.
Junior: It adds exception handling, headers, CORS, health, endpoints.
Mid-level: Exception middleware wraps later middleware (`src/NetTrello.Api/Program.cs:32-37`).
Senior: Order affects observability and security; CORS is not auth.

**Question:** What is optimistic concurrency here?
Junior: The board has a version.
Mid-level: Client sends `X-Board-Version` (`client/src/api/boardClient.ts:30-32`) and service rejects stale versions (`src/NetTrello.Api/Application/BoardService.cs:106-119`).
Senior: Header is optional today, so correctness depends on client discipline; require it for mutations.

## React

**Question:** Why have `useBoard`?
Junior: It loads boards.
Mid-level: It centralizes loading, saving, errors, actions (`client/src/hooks/useBoard.ts:5-159`).
Senior: It prevents scattered fetch logic but may grow into a store; conflict and stale closure paths need tests.

**Question:** What could go wrong in `CardEditor`?
Junior: The form might submit bad data.
Mid-level: Local state copies props at `client/src/components/CardEditor.tsx:14-20`.
Senior: If selected card changes while mounted, local state can stale; key modal by card id or sync carefully.

## Debugging

**Question:** A card move fails silently. How do you debug?
Junior: Check console and network.
Mid-level: Trace drag start/drop/client/API route/domain (`client/src/components/KanbanCard.tsx:15-27`, `client/src/components/KanbanColumn.tsx:28-33`, `src/NetTrello.Api/Domain/Board.cs:84-92`).
Senior: Separate UI event failure, server conflict, validation, and stale state; add regression coverage.

## System Design

**Question:** How would you make this production multi-user?
Junior: Add login.
Mid-level: Add auth, database, tests.
Senior: Model board membership, resource authorization, durable persistence with concurrency token, audit, observability, migration, rollback, and contract tests. Anchor current gaps to `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41` and `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:7-85`.

## Code Review

**Question:** Review a PR that returns `Board` directly from endpoints.
Junior: It might work.
Mid-level: It breaks DTO separation.
Senior: It leaks domain shape and creates API compatibility debt; current mapper exists at `src/NetTrello.Api/Contracts/BoardMapper.cs:17-54`.

## Behavioral Prompts

- Tell me about a time you reduced risk: use requiring version headers and adding conflict tests around `src/NetTrello.Api/Application/BoardService.cs:106-119`.
- Tell me about a time you improved maintainability: use API client centralization at `client/src/api/boardClient.ts:24-112`.
- Tell me about a time you challenged a design: use "CORS is not authorization" from `src/NetTrello.Api/Program.cs:19-28`.

