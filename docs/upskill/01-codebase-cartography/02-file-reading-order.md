# File Reading Order

## Junior Path

| Order | File | Why | What to look for |
| --- | --- | --- | --- |
| 1 | `README.md:1-60` | Names the repo and run model | What is verified vs inferred |
| 2 | `src/NetTrello.Api/Program.cs:7-39` | Shows how the API is assembled | DI, middleware order, endpoint mapping |
| 3 | `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41` | Shows every API route | Route, request DTO, service method |
| 4 | `src/NetTrello.Api/Application/BoardService.cs:17-43` | Read-only and create flows | Mapping and default columns |
| 5 | `src/NetTrello.Api/Application/BoardService.cs:46-119` | Mutations and conflict checking | `expectedVersion` and `MutateAsync` |
| 6 | `src/NetTrello.Api/Contracts/BoardDtos.cs:5-54` | HTTP contract | What the browser can depend on |
| 7 | `client/src/types.ts:1-65` | Client contract mirror | Nullable fields and enum strings |
| 8 | `client/src/api/boardClient.ts:24-112` | HTTP behavior | Headers, errors, method names |
| 9 | `client/src/hooks/useBoard.ts:19-147` | Data lifecycle | Effects, mutation wrapper, conflict refresh |
| 10 | `client/src/App.tsx:7-49` | UI composition | Which component owns which concern |

## Mid-Level Path

| Order | File | Why | What to look for |
| --- | --- | --- | --- |
| 11 | `src/NetTrello.Api/Domain/Board.cs:44-99` | Mutation model | Invariants and version increments |
| 12 | `src/NetTrello.Api/Domain/BoardColumn.cs:43-88` | Collection behavior | Reindexing and limits |
| 13 | `src/NetTrello.Api/Domain/Card.cs:48-82` | Card validation | Normalized labels and timestamps |
| 14 | `src/NetTrello.Api/Domain/Guard.cs:3-34` | Validation primitives | Required, optional, non-negative |
| 15 | `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:35-85` | Persistence boundary | Clone isolation and write lock |
| 16 | `src/NetTrello.Api/Contracts/BoardMapper.cs:7-54` | Mapping | Avoid leaking domain directly |
| 17 | `client/src/components/KanbanBoard.tsx:15-77` | Board UI ownership | Local modal and column state |
| 18 | `client/src/components/KanbanColumn.tsx:14-58` | Column drop behavior | Drag payload parsing |
| 19 | `client/src/components/KanbanCard.tsx:10-59` | Card drag behavior | Event propagation |
| 20 | `client/src/components/CardEditor.tsx:22-111` | Edit form | Form-to-contract transform |

## Senior Path

| Order | File | Why | What to critique |
| --- | --- | --- | --- |
| 21 | `src/NetTrello.Api/NetTrello.Api.csproj:1-9` | Build posture | Warnings as errors, nullable, target framework |
| 22 | `Directory.Build.props:1-7` | Shared build policy | Analysis level and style enforcement |
| 23 | `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:21-60` | Failure contract | Problem details, trace ids, logging |
| 24 | `src/NetTrello.Api/Middleware/SecurityHeadersMiddleware.cs:12-20` | Platform hardening | Headers present, headers missing |
| 25 | `src/NetTrello.Api/appsettings.json:1-8` | Environment boundary | CORS as config |
| 26 | `src/NetTrello.Api/Dockerfile:1-16` | Deployment model | Restore/publish/runtime split |
| 27 | `client/package.json:6-18` | Frontend toolchain | Scripts and dependency pinning risk |
| 28 | `client/tsconfig.json:2-20` | Type posture | Strict mode and module settings |
| 29 | `client/src/styles/app.css:1-342` | Product UX surface | Responsiveness and accessibility risks |
| 30 | `docs/api.http:1-29` | Manual API probes | Request examples and missing coverage |

