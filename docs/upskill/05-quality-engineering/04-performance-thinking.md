# Performance Thinking

Measure first. This repo is small enough that most "performance bugs" are hypotheses until tested.

## Likely Hotspots

| Area | Evidence | Why it could matter | Measure |
| --- | --- | --- | --- |
| Clone allocations | `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:38-45`, `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:77-79` | Large boards allocate on every read/write | Allocation profiler |
| Mapper traversal | `src/NetTrello.Api/Contracts/BoardMapper.cs:17-54` | Maps every column/card on every response | Benchmark board sizes |
| Board-level payload | Mutations return full board in `src/NetTrello.Api/Application/BoardService.cs:118-118` | Network grows with board size | Network panel |
| React full board render | `client/src/components/KanbanBoard.tsx:48-64` maps all columns/cards | Large boards re-render many cards | React profiler |
| Integer reindexing | `src/NetTrello.Api/Domain/BoardColumn.cs:74-80` | Moving one card rewrites positions | Benchmark long columns |
| Serial writes | `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:49-85` | One write lock for all boards | Load test |

## Drill

Pick one hotspot and write a measurement plan before changing code. Strong answer includes baseline, threshold, test data, and rollback.

