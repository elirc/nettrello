# Risk Register

| Risk | Evidence | Impact | Likelihood | Suggested test | Suggested fix | Confidence |
| --- | --- | --- | --- | --- | --- | --- |
| No tests | No test projects found | Regressions during refactor | High | Add domain/service/API tests | Create test projects | High |
| No auth | `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41` | Unauthorized mutation | High if public | IDOR integration tests | Add auth and resource policies | High |
| In-memory data loss | `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:7-85` | User data lost on restart | High | Restart persistence test | Add durable DB | High |
| Optional concurrency header | `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:46-59` | Silent overwrites by non-client callers | Medium | Missing-header mutation test | Require header on mutations | High |
| Manual DTO drift | `src/NetTrello.Api/Contracts/BoardDtos.cs:5-54`, `client/src/types.ts:1-65` | Runtime client/server mismatch | Medium | Contract snapshot/generation check | Generate client or contract tests | High |
| Clone allocation | `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:38-45`, `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:77-79` | Latency/GC on large boards | Medium later | Benchmark large board | Persist/query partial board data | Medium |
| No rate limiting | `src/NetTrello.Api/Program.cs:30-39` | Abuse of write endpoints | Medium if public | Load and abuse tests | Add rate limiting | High |
| Limited logs | `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:35-39` | Hard production diagnosis | Medium | Operational drill | Add structured logs and metrics | High |
| No lockfile | `client/package.json:12-18` | Non-reproducible installs | Medium | CI fresh install | Commit lockfile | High |
| No CSP | `src/NetTrello.Api/Middleware/SecurityHeadersMiddleware.cs:12-20` | Browser hardening incomplete | Low for API-only | Header integration test | Add CSP if serving HTML | Medium |

