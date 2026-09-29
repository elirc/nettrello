# Review Katas

## Kata 1: Require Board Version
**Author intent:** Prevent stale writes. **Fake diff summary:** Mutating endpoints reject missing `X-Board-Version`. **Resembles:** `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:46-59`. **Expected findings:** Blocking - update client methods in `client/src/api/boardClient.ts:64-111`; Important - migration path for existing clients. **Good comment:** > This tightens correctness, but please include a client update and one conflict regression test so we do not break current UI mutations.

## Kata 2: Add Column Rename
**Author intent:** Let users rename lanes. **Resembles:** `src/NetTrello.Api/Domain/BoardColumn.cs:33-36`. **Expected findings:** Blocking - route/service/board aggregate method missing; Important - version header. **Good comment:** > The column method exists, but the aggregate should expose rename so board version and updated timestamp stay consistent.

## Kata 3: Replace In-Memory With EF
**Author intent:** Make data durable. **Resembles:** `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:7-85`. **Expected findings:** Blocking - repository lifetime must change; Important - migrations and concurrency token. **Good comment:** > Please include a migration and update DI lifetime; a singleton repository cannot safely hold a scoped DbContext.

## Kata 4: Add Auth
**Author intent:** Only members edit boards. **Resembles:** `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41`, `src/NetTrello.Api/Domain/Board.cs:113-123`. **Expected findings:** Blocking - membership display is not caller identity. **Good comment:** > This checks assignees, but we still need caller authorization against `boardId` before mutation.

## Kata 5: Add Optimistic UI
**Author intent:** Make drag/drop feel instant. **Resembles:** `client/src/hooks/useBoard.ts:85-109`. **Expected findings:** Important - rollback on 409 and server validation. **Good comment:** > Optimistic updates are fine, but show the rollback path for `ApiError` 409 and card-limit conflicts.

## Kata 6: Add CSP Header
**Author intent:** Harden browser security. **Resembles:** `src/NetTrello.Api/Middleware/SecurityHeadersMiddleware.cs:12-20`. **Expected findings:** Optional for API-only, Important if serving client. **Good comment:** > CSP is useful if this process serves HTML; since the API currently does not, document the threat model and test header behavior.

## Kata 7: Add Generated Client
**Author intent:** Prevent DTO drift. **Resembles:** `src/NetTrello.Api/Contracts/BoardDtos.cs:5-54`, `client/src/types.ts:1-65`. **Expected findings:** Important - generation command and CI check. **Good comment:** > Good direction. Please add the generation command to docs and make CI fail when generated files are stale.

## Kata 8: Add Archive Restore
**Author intent:** Undo accidental archives. **Resembles:** `src/NetTrello.Api/Domain/Board.cs:95-99`. **Expected findings:** Blocking - current archive deletes active reference; no archive store. **Good comment:** > Restore needs a durable archived state, not only a UI undo, because the current domain removes the card.

