# Observability And Operations

## What Exists

- Health check route at `src/NetTrello.Api/Program.cs:36-36`.
- Exception logging for unhandled exceptions at `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:35-39`.
- Trace id emitted in problem details at `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:56-57`.
- Dockerfile for API runtime at `src/NetTrello.Api/Dockerfile:1-16`.

## What Is Missing

- Structured request logs for board mutations.
- Metrics for request count, latency, conflicts, validation errors.
- Distributed tracing.
- Readiness checks for future database.
- Deployment and rollback documentation.
- Client build/deploy path in Docker.

## How Would I Know This Broke?

| Flow | Signal to add |
| --- | --- |
| Initial board load | API GET latency and non-2xx count for `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:13-23` |
| Move card | Conflict count from `src/NetTrello.Api/Application/BoardService.cs:110-113` |
| Add card | Card-limit conflict count from `src/NetTrello.Api/Domain/BoardColumn.cs:82-88` |
| Error handling | Problem detail status/code distribution from `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:43-60` |

Drill: Add a logging plan for `MoveCardAsync` that does not log card descriptions or emails.

