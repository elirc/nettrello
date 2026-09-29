# Learning Rubrics

## Junior

Observable behaviors:

- Can run or explain inferred run commands from `client/package.json:6-10` and `src/NetTrello.Api/Properties/launchSettings.json:4-10`.
- Can trace one endpoint from route to service to domain, such as `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:37-38` -> `src/NetTrello.Api/Application/BoardService.cs:82-88` -> `src/NetTrello.Api/Domain/Board.cs:84-92`.
- Makes small changes in existing patterns without broad refactors.

Self-check:

- I can name the owner of UI state, HTTP paths, use cases, domain rules, and storage.
- I can explain one validation rule and where it is enforced.

## Mid-Level

Observable behaviors:

- Designs cross-layer changes with DTO, client type, endpoint, service, domain, persistence, and test impact.
- Identifies missing tests and writes targeted coverage.
- Debugs by narrowing layer boundaries.

Self-check:

- I can explain optimistic concurrency here using `client/src/api/boardClient.ts:30-32` and `src/NetTrello.Api/Application/BoardService.cs:106-119`.
- I can review a PR for blast radius and missing tests.

## Senior

Observable behaviors:

- Critiques architecture without rewriting for sport.
- Separates confirmed facts from hypotheses.
- Plans migrations, rollbacks, observability, and security.
- Mentors by making the next good change obvious.

Self-check:

- I can rank risks in `08-reference/risk-register.md`.
- I can design durable persistence from `src/NetTrello.Api/Application/IBoardRepository.cs:5-15` without coupling the UI to storage.
- I can explain who pays for each architectural decision.

