# Systematic Debugging

Method: reproduce, narrow, hypothesize, test cheaply, fix root cause, add regression coverage.

## Scenario: Dragging A Card Does Nothing

**Reproduction:** Drag a card onto another card or column. **First question:** UI event or API mutation?
**Narrowing path:** Check drag payload in `client/src/components/KanbanCard.tsx:15-18`; check drop parsing in `client/src/components/KanbanColumn.tsx:15-33`; check network call in `client/src/api/boardClient.ts:94-102`.
**Useful probes:** Browser devtools network panel, breakpoint in `onMoveCard`.
**Likely root causes:** Empty DataTransfer payload, `event.stopPropagation` missing, API 409.
**Regression test:** Component drag/drop test.
**Senior lesson:** Separate event failure from server rejection.

## Scenario: Card Move Returns 409

**Narrowing path:** Inspect header set at `client/src/api/boardClient.ts:30-32`; compare board version displayed at `client/src/components/KanbanBoard.tsx:36-45`; inspect conflict check at `src/NetTrello.Api/Application/BoardService.cs:106-119`.
**Likely root causes:** Stale board state from another tab, rapid actions using old closure.
**Regression test:** Service conflict test and UI refresh test.

## Scenario: Created Board Does Not Persist After Restart

**Narrowing path:** Confirm repository storage in `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:9-10`; confirm seed-only constructor in `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:12-32`.
**Likely root cause:** Expected behavior; no durable persistence.
**Regression test:** None unless product requires durability; then add persistence tests.

## Scenario: Client Cannot Reach API

**Narrowing path:** Check base URL at `client/src/api/boardClient.ts:3-3`; API launch URL at `src/NetTrello.Api/Properties/launchSettings.json:4-10`; CORS origins at `src/NetTrello.Api/appsettings.json:1-8`.
**Likely root causes:** API not running, wrong port, CORS origin mismatch.
**Regression test:** Integration smoke test.

## Scenario: Validation Error Is Too Generic

**Narrowing path:** Domain throws code in `src/NetTrello.Api/Domain/Guard.cs:3-34`; middleware writes problem details in `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:43-60`; client converts to `ApiError` in `client/src/api/boardClient.ts:39-48`; UI displays only message in `client/src/App.tsx:14-21`.
**Likely root cause:** No field-error model.
**Regression test:** API validation contract test and UI field error test.

