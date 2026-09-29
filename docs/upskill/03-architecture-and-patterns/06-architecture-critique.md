# Architecture Critique

## Strong Choices

- Thin endpoints delegate to `BoardService` (`src/NetTrello.Api/Endpoints/BoardEndpoints.cs:13-41`, `src/NetTrello.Api/Application/BoardService.cs:17-119`).
- Domain methods protect invariants instead of public setters (`src/NetTrello.Api/Domain/Board.cs:44-156`, `src/NetTrello.Api/Domain/Card.cs:48-62`).
- HTTP contracts are explicit records (`src/NetTrello.Api/Contracts/BoardDtos.cs:5-54`).
- Client has a centralized HTTP wrapper (`client/src/api/boardClient.ts:24-112`) and state hook (`client/src/hooks/useBoard.ts:5-159`).
- Optimistic concurrency exists at the service/client boundary (`src/NetTrello.Api/Application/BoardService.cs:106-119`, `client/src/api/boardClient.ts:30-32`).

## Risks And Tradeoffs

| Priority | Risk | Evidence | Impact | Recommendation |
| --- | --- | --- | --- | --- |
| 1 | No tests | No test projects found | Refactors are unsafe | Add API unit/integration tests around domain/service/endpoints |
| 2 | No auth | Endpoints lack authorization in `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41` | Any caller can mutate boards | Add authn/authz before persistence goes multi-user |
| 3 | In-memory storage | `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:7-85` | Data loss and single-process limit | Add EF Core or document DB with migrations |
| 4 | Optional version header | `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:46-59` | Clients can bypass conflict checks | Require header for mutations |
| 5 | Manual contract duplication | `src/NetTrello.Api/Contracts/BoardDtos.cs:5-54`, `client/src/types.ts:1-65` | Drift | Add contract tests or generated client |
| 6 | No CI | No workflow files found | Quality gates are local only | Add build/test/lint workflow |
| 7 | Limited observability | Only unhandled exceptions are logged at `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:35-39` | Harder production diagnosis | Add structured request/mutation logs and metrics |

## If I Owned This For Three Months

1. Week 1: add tests for domain and service behavior; prove card movement, card limit, assignee validation, conflicts.
2. Week 2: require board version on all mutations and make conflict UI clearer.
3. Month 1: introduce auth and board membership authorization.
4. Month 2: replace in-memory repository with durable persistence and migrations.
5. Month 3: add CI, Docker/client deployment story, logs, metrics, and a small RFC process.

## Confirmed vs Hypothesis

- Confirmed: no EF Core files, no test projects, no CI workflow in inventory.
- Confirmed: CORS and security headers exist (`src/NetTrello.Api/Program.cs:19-34`, `src/NetTrello.Api/Middleware/SecurityHeadersMiddleware.cs:12-20`).
- Hypothesis: clone-heavy repository will allocate heavily on large boards; measure before optimizing.
- Hypothesis: local form state in `CardEditor` may stale if selected card changes while modal remains mounted; verify with UI test.

