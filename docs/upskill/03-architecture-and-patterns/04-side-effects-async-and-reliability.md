# Side Effects, Async, And Reliability

## Current Side Effects

The app has no external side effects such as email, webhooks, queues, caches, or outbound HTTP clients. The only side effect is in-process mutation of repository state, serialized by `SemaphoreSlim` in `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:49-85`.

## Async Shape

- Endpoints pass cancellation tokens through `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:13-41`.
- Repository methods are async-shaped even when reads complete synchronously (`src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:35-46`).
- Writes await a lock with cancellation (`src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:49-69`).

## Reliability Concepts To Add Later

| Concept | Why it matters | Where it would attach |
| --- | --- | --- |
| Idempotency | Retried requests should not duplicate work | Create board/card endpoints in `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:16-32` |
| Outbox | Persist side effects with domain changes | Repository update path `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:67-85` or future DB transaction |
| Timeouts | External calls must not hang requests | Future `IHttpClientFactory` services |
| Retry policy | Transient failures need bounded retries | Future webhook/email worker |
| Observability | Operators need failure visibility | Current exception logging `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:35-39` |

## Drill

Imagine adding Slack notifications when a card moves. Strong design: domain records event, service saves board and event atomically, background worker sends Slack, retries with idempotency key, UI does not wait on Slack.

