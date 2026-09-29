# Command Cheatsheet

| Command | Purpose | Status | Evidence |
| --- | --- | --- | --- |
| `rg --files` | Inventory repo | Verified | Used during exploration |
| `git status --short -- .` | Check repo-local changes | Verified | Used during exploration |
| `dotnet run --project src/NetTrello.Api` | Run API | Inferred | Project file `src/NetTrello.Api/NetTrello.Api.csproj:1-9`; launch URL `src/NetTrello.Api/Properties/launchSettings.json:4-10` |
| `dotnet build NetTrello.sln` | Build API | Inferred, not run | `dotnet` unavailable locally |
| `dotnet test` | Run tests | Inferred gap | No test projects found |
| `cd client; npm install` | Install client deps | Not run by request | Dependencies listed in `client/package.json:12-18` |
| `cd client; npm run dev` | Run Vite dev server | Inferred | Script `client/package.json:6-10`, port `client/vite.config.ts:6-13` |
| `cd client; npm run build` | Typecheck and bundle | Inferred, not run | Script `client/package.json:8-8` |
| `cd client; npm run lint` | Typecheck only | Inferred, not run | Script `client/package.json:10-10` |
| `docker build -f src/NetTrello.Api/Dockerfile -t nettrello-api .` | Build API image | Inferred, not run | Dockerfile `src/NetTrello.Api/Dockerfile:1-16` |

## Migration Commands

No EF Core tooling exists yet. Future commands might include `dotnet ef migrations add ...`, but that would require package references and a `DbContext`, neither found in the current repo.

