# Data Model And Persistence

## Current Model

The aggregate is `Board`: id, name, description, timestamps, version, columns, members (`src/NetTrello.Api/Domain/Board.cs:8-35`). Columns contain ordered cards and a card limit (`src/NetTrello.Api/Domain/BoardColumn.cs:7-31`). Cards contain fields such as priority, assignee, due date, labels, position, and timestamps (`src/NetTrello.Api/Domain/Card.cs:5-46`).

## Current Persistence

Persistence is in-memory:

- `ConcurrentDictionary<Guid, Board>` stores boards at `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:9-9`.
- `SemaphoreSlim` serializes writes at `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:10-10`.
- Reads return clones at `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:35-46`.
- Writes clone before storing and returning at `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:49-85`.

## EF Core Track

No EF Core `DbContext`, migrations, or package references were found. If this repo adds EF Core, teach these decisions:

- DbContext lifetime should be scoped, not singleton, because `BoardService` is already scoped in `src/NetTrello.Api/Program.cs:11-13`.
- Board version should become a concurrency token; current checks live in `src/NetTrello.Api/Application/BoardService.cs:106-119`.
- `BoardColumn.Position` and `Card.Position` need indexes if lists are queried/sorted often; positions are reindexed in `src/NetTrello.Api/Domain/Board.cs:144-150` and `src/NetTrello.Api/Domain/BoardColumn.cs:74-80`.
- Archive semantics need a durable model. Today archive removes a card from active columns in `src/NetTrello.Api/Domain/Board.cs:95-99`.

## Safe Schema-Change Guidance

1. Add nullable columns or new tables first.
2. Backfill data.
3. Deploy code that reads both old and new shape.
4. Switch writes.
5. Remove old shape after verification.

Drill: Design a `CardArchivedAt` migration. Strong answer includes API behavior, backfill, tests, and rollback.

