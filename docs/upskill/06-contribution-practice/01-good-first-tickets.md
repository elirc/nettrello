# Good-First Tickets

## Ticket 1: Make Board Version Header Required
**Difficulty:** Easy **Time:** 1-2 hours **Skills:** API contracts, errors. **Story:** As an API maintainer, I want mutation requests to include board version so stale writes are rejected. **Read first:** `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:46-59`. **Files likely touched:** endpoint helper, client calls at `client/src/api/boardClient.ts:64-111`. **Acceptance criteria:** missing header on mutation returns 400; client still sends header.

## Ticket 2: Add Column Rename Endpoint
**Difficulty:** Easy **Time:** 2 hours **Skills:** endpoint/service/domain. **Read first:** `src/NetTrello.Api/Domain/BoardColumn.cs:33-36`. **Files likely touched:** `Board.cs`, `BoardService.cs`, `BoardEndpoints.cs`, `boardClient.ts`. **Acceptance criteria:** column title changes, board version increments, empty title rejected.

## Ticket 3: Add Card Description To Composer
**Difficulty:** Easy **Time:** 1 hour **Skills:** React forms. **Read first:** `client/src/components/CardComposer.tsx:18-35`. **Acceptance criteria:** composer sends description; backend validation remains in `src/NetTrello.Api/Domain/Card.cs:17-18`.

## Ticket 4: Show Due Date On Card
**Difficulty:** Easy **Time:** 1 hour **Skills:** UI formatting. **Read first:** `client/src/components/KanbanCard.tsx:29-43`, `client/src/types.ts:29-40`. **Acceptance criteria:** due date appears when present and does not overflow small cards.

## Ticket 5: Improve Conflict Message
**Difficulty:** Easy **Time:** 1 hour **Skills:** error UX. **Read first:** `client/src/hooks/useBoard.ts:98-103`, `client/src/App.tsx:14-21`. **Acceptance criteria:** 409 shows refresh-specific copy.

## Ticket 6: Add Empty Column Drop Hint
**Difficulty:** Easy **Time:** 1 hour **Skills:** React/CSS. **Read first:** `client/src/components/KanbanColumn.tsx:36-55`. **Acceptance criteria:** empty columns remain valid drop targets.

## Ticket 7: Add `aria-label` To Card Buttons
**Difficulty:** Easy **Time:** 30 minutes **Skills:** accessibility. **Read first:** `client/src/components/KanbanCard.tsx:29-32`. **Acceptance criteria:** screen reader name includes card title.

## Ticket 8: Document API Error Codes
**Difficulty:** Easy **Time:** 1 hour **Skills:** docs. **Read first:** `src/NetTrello.Api/Domain/DomainException.cs:3-20`, `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:43-60`. **Acceptance criteria:** docs list known code values and meanings.

## Ticket 9: Add Board Summary Updated Time
**Difficulty:** Easy **Time:** 1 hour **Skills:** UI. **Read first:** `client/src/components/BoardSidebar.tsx:31-40`, `src/NetTrello.Api/Contracts/BoardDtos.cs:5-5`. **Acceptance criteria:** sidebar shows relative or formatted updated time.

## Ticket 10: Extract Priority Options
**Difficulty:** Easy **Time:** 1 hour **Skills:** duplication removal. **Read first:** `client/src/components/CardComposer.tsx:4-4`, `client/src/components/CardEditor.tsx:4-4`. **Acceptance criteria:** one shared constant with no behavior change.

## Ticket 11: Add Manual API Example For Move Card
**Difficulty:** Easy **Time:** 30 minutes **Skills:** API docs. **Read first:** `docs/api.http:1-29`, `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:37-38`. **Acceptance criteria:** example includes `X-Board-Version`.

## Ticket 12: Add Local Time Formatting Helper
**Difficulty:** Easy **Time:** 1 hour **Skills:** frontend organization. **Read first:** `client/src/components/KanbanBoard.tsx:36-45`. **Acceptance criteria:** date formatting is centralized and tested when test framework exists.

## Ticket 13: Add Health Check Note To README
**Difficulty:** Easy **Time:** 30 minutes **Skills:** docs. **Read first:** `src/NetTrello.Api/Program.cs:36-36`. **Acceptance criteria:** README documents `/health`.

## Ticket 14: Add Problem Details Screenshot Scenario
**Difficulty:** Easy **Time:** 1 hour **Skills:** debugging docs. **Read first:** `client/src/api/boardClient.ts:39-48`. **Acceptance criteria:** docs explain how to inspect failed response.

## Ticket 15: Add Keyboard Card Move Buttons
**Difficulty:** Medium **Time:** 3 hours **Skills:** accessible UI. **Read first:** `client/src/components/KanbanCard.tsx:10-59`, `client/src/components/KanbanColumn.tsx:28-33`. **Acceptance criteria:** card can move without mouse.

## Ticket 16: Add Domain Unit Test Project
**Difficulty:** Medium **Time:** 3 hours **Skills:** testing. **Read first:** `src/NetTrello.Api/Domain/Board.cs:44-156`. **Acceptance criteria:** tests cover add/move/archive and card limit.

