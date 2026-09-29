# Annotation Drills

1. Annotate `src/NetTrello.Api/Program.cs:7-39`: mark service lifetimes, middleware order, endpoint mapping, and config dependency.
2. Annotate `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41`: for each route, list method, route ids, body DTO, service method, and response shape.
3. Annotate `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:46-59`: input, output, error condition, and why optional version is a risk.
4. Annotate `src/NetTrello.Api/Application/BoardService.cs:106-119`: identify concurrency boundary, mutation delegate, repository ownership, and conflict failure.
5. Annotate `src/NetTrello.Api/Domain/Board.cs:66-73`: list invariants before and after adding a card.
6. Annotate `src/NetTrello.Api/Domain/Board.cs:84-92`: trace source column, removed card, target column, reindexing, and version touch.
7. Annotate `src/NetTrello.Api/Domain/BoardColumn.cs:43-48`: explain insert position, card limit, and reindexing.
8. Annotate `src/NetTrello.Api/Domain/Card.cs:75-82`: explain normalization, distinctness, sort order, and max-length enforcement.
9. Annotate `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:67-85`: identify lock scope, clone boundary, and exception behavior.
10. Annotate `client/src/hooks/useBoard.ts:85-109`: mark loading state, error state, mutation function, conflict recovery, and stale closure risk.
11. Annotate `client/src/components/KanbanCard.tsx:15-27`: identify browser drag payload, event propagation, and drop target responsibility.
12. Annotate `client/src/components/CardEditor.tsx:22-35`: mark form-to-contract transformations and timezone risk.

Rubric: Basic names inputs/outputs. Solid explains ownership. Strong writes one regression test per drill.

