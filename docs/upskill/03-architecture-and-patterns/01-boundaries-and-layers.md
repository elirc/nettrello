# Boundaries And Layers

## What Each Layer Owns

| Layer | Owns | Evidence | Must not own |
| --- | --- | --- | --- |
| React components | Rendering, local form state, user events | `client/src/components/CardComposer.tsx:11-75`, `client/src/components/KanbanCard.tsx:10-59` | API paths, domain invariants |
| Hook | Fetch lifecycle, saving state, conflict handling | `client/src/hooks/useBoard.ts:19-147` | HTTP string parsing, server rules |
| API client | Base URL, headers, problem parsing, paths | `client/src/api/boardClient.ts:24-112` | UI state |
| Endpoints | Routes, model binding, request headers | `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-60` | Business invariants |
| Application | Use-case sequencing, concurrency checks | `src/NetTrello.Api/Application/BoardService.cs:46-119` | HTTP response formatting |
| Domain | Invariants and aggregate state | `src/NetTrello.Api/Domain/Board.cs:44-156` | Persistence mechanics |
| Infrastructure | Storage implementation | `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:7-85` | Product policy |

## Boundary Leaks To Watch

- Domain exceptions carry HTTP status numbers in `src/NetTrello.Api/Domain/DomainException.cs:3-20`. That is acceptable for a tiny app, but a larger domain might use domain-only error kinds and map them at the API boundary.
- Client and server contracts are manually duplicated (`client/src/types.ts:1-65`, `src/NetTrello.Api/Contracts/BoardDtos.cs:5-54`). Without tests or generated clients, drift is a real risk.
- The repository is in-memory and seeded with members in infrastructure (`src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:12-32`), so product seed policy is mixed with storage.

## Drill

For a feature "rename column", decide which layer changes first. Strong answer: DTO and endpoint for transport, service for use case, domain method already exists at `src/NetTrello.Api/Domain/BoardColumn.cs:33-36` but board-level method is missing, hook/client method, UI form, tests.

