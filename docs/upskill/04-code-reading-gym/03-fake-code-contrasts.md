# Fake Code Contrasts

## 1. DTO Coupled To Entity

Illustrative fake code: not from this repo.

```csharp
app.MapGet("/api/boards/{id}", (Board board) => board);
```

Better here: map through DTOs at `src/NetTrello.Api/Contracts/BoardMapper.cs:17-54` so domain shape can change without breaking clients.

## 2. Missing Version Header

Illustrative fake code: not from this repo.

```ts
fetch(`/api/boards/${boardId}/cards/${cardId}/move`, { method: "PUT", body });
```

Better here: `boardClient` sends `X-Board-Version` in `client/src/api/boardClient.ts:30-32`.

## 3. Business Logic In Endpoint

Illustrative fake code: not from this repo.

```csharp
boards.MapPost("/{id}/cards", request => { /* find board, validate, mutate */ });
```

Better here: endpoints delegate to `BoardService` at `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:25-41`.

## 4. Public Setters Everywhere

Illustrative fake code: not from this repo.

```csharp
card.Title = "";
card.Position = -1;
```

Better here: private setters and methods enforce guard rules in `src/NetTrello.Api/Domain/Card.cs:28-62`.

## 5. Swallowed Exceptions

Illustrative fake code: not from this repo.

```csharp
catch { return Results.BadRequest(); }
```

Better here: middleware preserves status, code, detail, trace id in `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:27-60`.

## 6. Client Fetch Everywhere

Illustrative fake code: not from this repo.

```tsx
await fetch("/api/boards");
```

Better here: one HTTP wrapper in `client/src/api/boardClient.ts:24-112`.

## 7. Stale Async State

Illustrative fake code: not from this repo.

```ts
useEffect(() => { boardClient.getBoard(id).then(setBoard); }, [id]);
```

Better here: `isCurrent` cleanup gates late responses in `client/src/hooks/useBoard.ts:51-83`.

## 8. Insecure UI-Only Permission

Illustrative fake code: not from this repo.

```tsx
{isAdmin && <button>Archive</button>}
```

Better future: enforce authorization server-side on endpoints currently open at `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41`.

## 9. Unbounded External Side Effects

Illustrative fake code: not from this repo.

```csharp
await slack.SendAsync("card moved");
board.MoveCard(...);
```

Better future: persist board and outbox event together around `src/NetTrello.Api/Application/BoardService.cs:82-88`; send asynchronously.

## 10. Contract Drift

Illustrative fake code: not from this repo.

```ts
type CardPriority = "low" | "medium";
```

Better here: match server enum strings in `src/NetTrello.Api/Domain/CardPriority.cs:1-8` and `client/src/types.ts:1-1`.

