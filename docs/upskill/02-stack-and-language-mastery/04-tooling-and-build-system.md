# Tooling And Build System

## .NET Build

The API uses the Web SDK in `src/NetTrello.Api/NetTrello.Api.csproj:1-1`, targets .NET 8 in `src/NetTrello.Api/NetTrello.Api.csproj:3-3`, enables nullable references and implicit usings in `src/NetTrello.Api/NetTrello.Api.csproj:4-5`, and treats warnings as errors in `src/NetTrello.Api/NetTrello.Api.csproj:6-6`.

Shared build properties live in `Directory.Build.props:1-7`. There is no `global.json`, so the installed SDK selection is machine-dependent. There is no `Directory.Packages.props`, and central package management is disabled in `Directory.Build.props:5-5`.

## Frontend Build

Npm scripts are defined in `client/package.json:6-10`. `npm run build` runs `tsc && vite build`, so TypeScript failures should block bundles. `client/tsconfig.json:10-17` enables strict TypeScript and React JSX.

## Docker

The Dockerfile restores the project before copying the full repo (`src/NetTrello.Api/Dockerfile:1-8`) and runs the published API from the ASP.NET runtime image (`src/NetTrello.Api/Dockerfile:10-16`). It builds only the API, not the React client.

## Gaps To Notice

- No lockfile found for npm dependencies. This weakens reproducibility.
- No test projects found.
- No CI workflow found.
- No `global.json` found, so SDK version is not pinned.

Drill: Write a PR checklist item for each gap and explain its blast radius.

