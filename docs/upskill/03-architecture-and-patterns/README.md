# Architecture And Patterns

This repo uses a small layered architecture:

1. Browser components render UI and collect input (`client/src/components/BoardSidebar.tsx:15-57`, `client/src/components/CardEditor.tsx:22-111`).
2. A hook orchestrates client state and mutations (`client/src/hooks/useBoard.ts:5-159`).
3. An API client owns transport details (`client/src/api/boardClient.ts:24-112`).
4. Minimal API endpoints own HTTP routing (`src/NetTrello.Api/Endpoints/BoardEndpoints.cs:9-60`).
5. Application services own use cases (`src/NetTrello.Api/Application/BoardService.cs:17-119`).
6. Domain classes own invariants (`src/NetTrello.Api/Domain/Board.cs:44-156`).
7. Infrastructure owns storage (`src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:7-85`).

Use these boundaries when deciding where new code belongs.

