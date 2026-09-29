# Codebase Cartography

This section teaches you how to build a map before changing code. The map is small but high signal: API composition in `src/NetTrello.Api/Program.cs:7-39`, endpoint contracts in `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41`, use cases in `src/NetTrello.Api/Application/BoardService.cs:17-119`, domain invariants in `src/NetTrello.Api/Domain/`, and React orchestration in `client/src/hooks/useBoard.ts:5-159`.

Read the files in order, draw the data flow, and mark every boundary where the data shape changes.

