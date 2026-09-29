# Mid-Level Feature Tickets

## Ticket 1: Durable Persistence With EF Core
**Difficulty:** Hard **Time:** 1-2 weeks. **Story:** As a user, I want boards to survive restarts. **Read first:** `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:7-85`. **Plan:** Add DbContext, migrations, scoped repository, concurrency token, integration tests. **Risk:** Data model commits future migrations.

## Ticket 2: Board Membership Authorization
**Difficulty:** Hard **Time:** 1 week. **Read first:** `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41`, `src/NetTrello.Api/Domain/Board.cs:113-123`. **Plan:** Add auth, caller identity, membership lookup, resource authorization tests. **Risk:** IDOR if any mutation skips policy.

## Ticket 3: Generated API Client
**Difficulty:** Medium **Time:** 2-4 days. **Read first:** `src/NetTrello.Api/Contracts/BoardDtos.cs:5-54`, `client/src/types.ts:1-65`. **Plan:** Add OpenAPI, generate TS client, CI drift check. **Risk:** Generated code churn.

## Ticket 4: Conflict Resolution UI
**Difficulty:** Medium **Time:** 3-5 days. **Read first:** `client/src/hooks/useBoard.ts:98-103`, `src/NetTrello.Api/Application/BoardService.cs:106-119`. **Plan:** Show stale-change dialog, preserve attempted action, retry after refresh. **Risk:** Replaying unsafe mutations.

## Ticket 5: Card Archive And Restore
**Difficulty:** Medium **Time:** 4-6 days. **Read first:** `src/NetTrello.Api/Domain/Board.cs:95-99`. **Plan:** Add archived collection/state, restore endpoint, UI archive view. **Risk:** Breaks current delete-like semantics.

## Ticket 6: Activity Feed
**Difficulty:** Medium **Time:** 1 week. **Read first:** `src/NetTrello.Api/Domain/Board.cs:152-156`. **Plan:** Record events for board mutations, expose feed endpoint, render UI. **Risk:** Event ordering and persistence.

## Ticket 7: Search And Filter Cards
**Difficulty:** Medium **Time:** 3-5 days. **Read first:** `client/src/components/KanbanBoard.tsx:48-64`, `src/NetTrello.Api/Contracts/BoardMapper.cs:17-54`. **Plan:** Client filter first, then server query after DB. **Risk:** Large board performance.

## Ticket 8: Rate Limiting
**Difficulty:** Medium **Time:** 2 days. **Read first:** `src/NetTrello.Api/Program.cs:30-39`. **Plan:** Add ASP.NET rate limiting middleware, tests for mutation routes. **Risk:** Blocking legitimate bursty drag/drop.

## Ticket 9: Observability Baseline
**Difficulty:** Medium **Time:** 3 days. **Read first:** `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:35-39`. **Plan:** Add structured logs for mutations, health details, metrics counters. **Risk:** Logging sensitive data.

## Ticket 10: Client Test Harness
**Difficulty:** Medium **Time:** 2-4 days. **Read first:** `client/package.json:6-19`. **Plan:** Add Vitest/RTL, test forms and hook with mocked client. **Risk:** Brittle tests around implementation details.

## Ticket 11: API Integration Test Harness
**Difficulty:** Medium **Time:** 2-4 days. **Read first:** `src/NetTrello.Api/Program.cs:41-43`. **Plan:** Add xUnit and WebApplicationFactory. **Risk:** In-memory singleton state leaking across tests.

## Ticket 12: Split Client Docker Build
**Difficulty:** Medium **Time:** 2-3 days. **Read first:** `src/NetTrello.Api/Dockerfile:1-16`, `client/package.json:6-10`. **Plan:** Add client image or static hosting story. **Risk:** Env injection at build vs runtime.

