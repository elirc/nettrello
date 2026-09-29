# Architecture

NetTrello uses a modest clean architecture split. The point is not ceremony; the point is to keep business behavior independent from delivery and persistence details.

## Backend Flow

1. Minimal API endpoints accept HTTP requests and read transport concerns such as headers.
2. `BoardService` executes use cases and checks optimistic concurrency.
3. Domain entities enforce invariants like required names, card limits, valid assignees, and reindexing after moves.
4. `IBoardRepository` abstracts storage. The current implementation is in-memory and thread-safe for local development.
5. DTO mappers return a stable response shape to the browser.

## Frontend Flow

1. `boardClient` centralizes fetch, headers, problem-detail parsing, and API paths.
2. `useBoard` loads data, runs mutations, handles conflicts, and refreshes summaries.
3. Components render focused UI slices: sidebar, board, columns, cards, editor, and composer.
4. Drag/drop sends the intended final state to the API. The server remains the source of truth.

## Enterprise Hardening Checklist

- Authentication: issue tokens from an identity provider and validate them in API middleware.
- Authorization: check board membership and role permissions per endpoint.
- Persistence: store boards, columns, cards, labels, members, and audit events in a durable database.
- Observability: add structured logs, metrics, traces, and correlation ids at ingress.
- Reliability: add endpoint integration tests, repository contract tests, and frontend tests for critical workflows.
- Deployment: split API and client images, pin dependencies, scan images, and run database migrations explicitly.
