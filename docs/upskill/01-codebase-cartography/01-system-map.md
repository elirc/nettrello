# System Map

## Project Graph

| Area | Owner | Evidence | Public or private |
| --- | --- | --- | --- |
| Solution | One API project | `NetTrello.sln:1-18`, `src/NetTrello.Api/NetTrello.Api.csproj:1-9` | Build metadata |
| API host | ASP.NET Core minimal API | `src/NetTrello.Api/Program.cs:7-39` | Public HTTP process |
| API endpoints | Board HTTP routes | `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41` | Public HTTP surface |
| Application layer | Board use cases | `src/NetTrello.Api/Application/BoardService.cs:17-119` | Internal service boundary |
| Domain | Board, column, card rules | `src/NetTrello.Api/Domain/Board.cs:44-99`, `src/NetTrello.Api/Domain/BoardColumn.cs:43-88`, `src/NetTrello.Api/Domain/Card.cs:48-62` | Internal business model |
| Persistence | In-memory repository | `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:7-85` | Replaceable infrastructure |
| Client app | React shell | `client/src/App.tsx:7-49`, `client/src/main.tsx:1-9` | Browser UI |
| Client state | Board hook | `client/src/hooks/useBoard.ts:5-159` | UI orchestration |
| Client HTTP | Typed fetch wrapper | `client/src/api/boardClient.ts:24-112` | Browser-to-API boundary |
| Tests | None found | No `*Tests*.csproj` files in inventory | Gap |
| CI | None found | No `.github/workflows` directory in inventory | Gap |

## Runtime Boundaries

- Browser runtime: React renders `App` and child components from `client/src/main.tsx:1-9`.
- API runtime: ASP.NET Core builds DI and middleware in `src/NetTrello.Api/Program.cs:7-39`.
- Storage runtime: repository data lives in process memory, seeded in `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:12-32`.
- Cross-origin boundary: allowed origins are configured in `src/NetTrello.Api/appsettings.json:1-8` and consumed in `src/NetTrello.Api/Program.cs:19-28`.

## Ownership Rules

- Components render and gather form input; see `client/src/components/CardEditor.tsx:22-35` and `client/src/components/CardComposer.tsx:18-35`.
- `useBoard` owns fetch timing, errors, saving state, and conflict refresh; see `client/src/hooks/useBoard.ts:19-147`.
- `boardClient` owns paths, headers, JSON, and error parsing; see `client/src/api/boardClient.ts:24-50`.
- `BoardService` owns use-case sequencing and optimistic concurrency; see `src/NetTrello.Api/Application/BoardService.cs:106-119`.
- Domain objects own invariants and state mutation; see `src/NetTrello.Api/Domain/Board.cs:113-156`.

## Verification Notes

The graph is based on `rg --files`, metadata inspection, and line-numbered reads. No external services were contacted.

