# Runtime And Tooling Map

## .NET

| Topic | Evidence | Learning note |
| --- | --- | --- |
| Target framework | `src/NetTrello.Api/NetTrello.Api.csproj:1-8` | The API targets .NET 8 with nullable references and warnings as errors. |
| Shared build settings | `Directory.Build.props:1-7` | Shared MSBuild policy applies to projects under the repo. |
| Launch URL | `src/NetTrello.Api/Properties/launchSettings.json:4-10` | Local API is intended to run at `http://localhost:5088`. |
| Docker | `src/NetTrello.Api/Dockerfile:1-16` | Multi-stage build publishes the API then runs from ASP.NET runtime image. |

## React And Vite

| Topic | Evidence | Learning note |
| --- | --- | --- |
| Dev scripts | `client/package.json:6-10` | `dev`, `build`, `preview`, and `lint` are npm scripts. |
| Dependencies | `client/package.json:12-18` | React, React DOM, Vite, TS, and React plugin are direct dependencies. |
| TS strictness | `client/tsconfig.json:2-20` | `strict: true` makes nullability and contract drift visible. |
| Dev port | `client/vite.config.ts:4-13` | Port is strict, so occupied port should fail rather than silently moving. |
| API base URL | `client/src/api/boardClient.ts:3-3` | Browser can override API URL with `VITE_API_BASE_URL`. |

## Runtime Boundaries

- Browser renders React and calls `fetch` in `client/src/api/boardClient.ts:34-50`.
- API receives JSON via minimal API endpoints in `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:13-41`.
- Domain mutation happens server-side in `src/NetTrello.Api/Domain/Board.cs:44-99`.
- In-memory persistence means restart loses user-created data; seed happens in `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:12-32`.

## Secrets And Environments

No secret management is present. CORS origins are plain config in `src/NetTrello.Api/appsettings.json:1-8`, and client API URL is env-driven in `client/.env.example:1-1`.

## Verification Notes

`node` and `npm` were found locally. `dotnet` was not found, so .NET commands are inferred from project files.

