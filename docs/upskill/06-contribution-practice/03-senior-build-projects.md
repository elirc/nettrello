# Senior Build Projects

## Project 1: Multi-User Board Security
**Value:** Make the app safe for real teams. **Checklist:** auth provider, board membership model, resource policies, IDOR tests, audit logs. **Anchors:** open endpoints `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41`; member display data `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:15-22`. **Rollout:** feature flag auth in dev, migrate data, enforce policies, monitor 403s.

## Project 2: Durable Persistence Migration
**Value:** Prevent data loss. **Checklist:** schema, migrations, concurrency token, indexes, repository lifetime, data backfill, rollback. **Anchors:** repository interface `src/NetTrello.Api/Application/IBoardRepository.cs:5-15`; current implementation `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:7-85`. **Rollout:** dual-write not needed for fresh app; for existing data, export/import seed.

## Project 3: Real-Time Collaboration
**Value:** Users see board changes without refresh. **Checklist:** SignalR, version protocol, conflict strategy, reconnect behavior, auth. **Anchors:** version field `src/NetTrello.Api/Domain/Board.cs:31-31`; hook conflict refresh `client/src/hooks/useBoard.ts:98-103`.

## Project 4: Reliable Eventing And Notifications
**Value:** Notify assignees without slowing writes. **Checklist:** domain events, outbox, worker, retries, idempotency, observability. **Anchors:** mutation methods `src/NetTrello.Api/Domain/Board.cs:44-99`; service use cases `src/NetTrello.Api/Application/BoardService.cs:46-97`.

## Project 5: Contract Generation And CI
**Value:** Stop API/client drift. **Checklist:** OpenAPI, generated TS client, build check, semantic API change policy. **Anchors:** server DTOs `src/NetTrello.Api/Contracts/BoardDtos.cs:5-54`; client mirror `client/src/types.ts:1-65`.

## Project 6: Large Board Performance
**Value:** Keep boards usable at thousands of cards. **Checklist:** pagination/windowing, partial mutation responses, indexes, profiling, payload budgets. **Anchors:** full-board mapper `src/NetTrello.Api/Contracts/BoardMapper.cs:17-54`; full render `client/src/components/KanbanBoard.tsx:48-64`.

## Project 7: Operational Readiness
**Value:** Make failures visible and recoverable. **Checklist:** logs, metrics, traces, health/readiness, dashboards, deployment and rollback docs. **Anchors:** health route `src/NetTrello.Api/Program.cs:36-36`; exception logs `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:35-39`.

