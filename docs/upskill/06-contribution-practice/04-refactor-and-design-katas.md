# Refactor And Design Katas

1. Boundary leak kata: remove HTTP status numbers from `src/NetTrello.Api/Domain/DomainException.cs:3-20`; map them in middleware.
2. Outbox kata: design event recording around `src/NetTrello.Api/Application/BoardService.cs:82-88` without sending notifications in request path.
3. Type-safety kata: generate `client/src/types.ts:1-65` from `src/NetTrello.Api/Contracts/BoardDtos.cs:5-54`.
4. N+1 kata: imagine EF replaces mapper logic at `src/NetTrello.Api/Contracts/BoardMapper.cs:17-54`; write eager-loading rules.
5. Testability kata: replace `DateTimeOffset.UtcNow` seed at `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:14-14` with `IClock`.
6. Accessibility kata: add keyboard card movement to mouse drag behavior at `client/src/components/KanbanCard.tsx:15-27`.
7. RFC kata: propose durable archive, starting from current removal at `src/NetTrello.Api/Domain/Board.cs:95-99`.
8. Review kata: critique optional concurrency header at `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:46-59`.

Self-grade: Strong answers include migration, tests, rollback, and how to limit blast radius.

