# Fast Track: Weekend Win

## Goal

By Sunday evening, you should be able to run or reason about the app, trace one UI-to-domain feature, make one safe change, and explain why this repo separates components, hooks, HTTP, services, domain, and storage.

## Commands

| Command | Status | Evidence |
| --- | --- | --- |
| `dotnet run --project src/NetTrello.Api` | Inferred, not run here | API launch URL is `http://localhost:5088` in `src/NetTrello.Api/Properties/launchSettings.json:4-10`; `dotnet` was not on PATH during verification. |
| `cd client; npm install; npm run dev` | Inferred, not run here | Scripts are declared in `client/package.json:6-10`; Vite dev port is fixed at `client/vite.config.ts:6-13`. |
| `npm run lint` | Inferred, not run here | The script maps to `tsc --noEmit` in `client/package.json:6-10`; dependencies were not installed. |
| `node -e "JSON.parse(...)"` | Verified | JSON metadata parse is logged in `08-reference/verification-log.md`. |

## First 10 Files

1. `README.md:1-60` - repo purpose and run notes.
2. `src/NetTrello.Api/Program.cs:7-39` - API composition root.
3. `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:9-60` - public HTTP surface.
4. `src/NetTrello.Api/Application/BoardService.cs:17-119` - use cases and version conflicts.
5. `src/NetTrello.Api/Domain/Board.cs:44-99` - board mutations and invariants.
6. `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:35-85` - storage isolation and write lock.
7. `client/src/App.tsx:7-49` - shell composition.
8. `client/src/hooks/useBoard.ts:19-147` - client data flow and mutation orchestration.
9. `client/src/api/boardClient.ts:24-112` - HTTP wrapper, headers, API paths.
10. `client/src/components/KanbanCard.tsx:10-59` - drag/drop card interaction.

## Trace This Flow

Trace "move a card" from `client/src/components/KanbanCard.tsx:15-27` to `client/src/components/KanbanColumn.tsx:28-33`, then to `client/src/hooks/useBoard.ts:139-140`, `client/src/api/boardClient.ts:94-102`, `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:37-38`, `src/NetTrello.Api/Application/BoardService.cs:82-88`, and `src/NetTrello.Api/Domain/Board.cs:84-92`.

## Safe First Change

Change the default new card priority in `client/src/components/CardComposer.tsx:12-16` from `Medium` to `Low`, then explain why the backend still accepts only the enum values represented in `src/NetTrello.Api/Domain/CardPriority.cs:1-8` and serialized as strings by `src/NetTrello.Api/Program.cs:14-17`.

## Teach-Back

Explain this in five sentences: "The browser sends a desired board mutation; the API checks version and domain rules; the repository saves a cloned board; the API returns the whole board; the hook refreshes summary state." Anchor each sentence to one file line.

## What This Skips

This fast track skips tests, durable persistence, authentication, deployment, and architecture critique. Those are covered later as learning projects because the repo currently has no test project, EF Core files, auth handlers, CI workflow, or hosted service.

## Verification Notes

Repo inventory was performed with `rg --files`. `dotnet` was unavailable. `node` and `npm` were available, but dependencies were not installed.

