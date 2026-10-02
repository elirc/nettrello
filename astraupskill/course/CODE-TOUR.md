# Code tour: the concurrent card edit, in two stacks

This tour follows one promise through NetTrello, with gitjira-rebuild
(`astrafinalorganize/gitjira-rebuild`) as the constant point of comparison:

> **When two people edit or move the same card at the same time, the second
> write must not silently erase the first — and the loser should not lose
> their work.**

gitjira keeps the whole promise. NetTrello keeps part of it, under conditions
this tour makes precise. Line numbers refer to the current source; if they
drift, search for the quoted code.

## 0. Set the scene

Imagine the seeded "Product launch" board open in two browser tabs. Both tabs
loaded the board at `version: N`. Tab A renames the card "Interview beta
customers"; tab B simultaneously changes its priority. In gitjira this story
ends with one 200, one 409, and tab B's draft intact on screen. Predict, before
reading on: where does NetTrello's version of this story diverge?

## 1. The client sends the version it read

- `client/src/hooks/useBoard.ts:136-137` — `updateCard` calls
  `boardClient.updateCard(current.id, cardId, input, current.version)`: the
  version from the board state the user is looking at, same idea as gitjira's
  `saveIssue()` spreading `{ version: editing.version }` into the PATCH body.
- `client/src/api/boardClient.ts:30-32` — the version travels as an
  `X-Board-Version` header, not in the body. Note what is different from
  gitjira already: the version is **board-wide**, not per-card. Editing card X
  and moving card Y are conflicts with each other here; in gitjira two
  different issues never conflict.

## 2. The server reads the header — and the first crack appears

- `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:34-35` — the PATCH route
  hands `ReadExpectedVersion(httpRequest)` to the service.
- `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:46-51` — **a missing header
  returns `null`, and `null` means "don't check"** (step 3 shows why). Any
  client that simply omits `X-Board-Version` — curl, a buggy mobile app, the
  repo's own `docs/api.http` requests — writes unconditionally. In gitjira the
  version is a required, validated field of the PATCH body; there is no opt-out.
  This is read-in-source fact, and exercise 1.2 turns it into a demonstrated
  lost update; exercise 2.1 closes it.
- A malformed header is at least rejected:
  `BoardEndpoints.cs:53-56` throws `board.version_invalid` (400).

## 3. The check runs in application code, not in storage

- `src/NetTrello.Api/Application/BoardService.cs:108-121` — `MutateAsync` is
  the single funnel every write goes through (column add/move, card
  add/update/move/archive). Inside the repository callback:

  ```csharp
  if (expectedVersion is not null && board.Version != expectedVersion.Value)
  {
      throw DomainException.Conflict("board.version_conflict", ...);
  }
  ```

  Compare gitjira's enforcement, `src/app.js:277`: `UPDATE ... WHERE id=? AND
  version=?` — the check and the write are **one atomic statement**, and
  SQLite's `BEGIN IMMEDIATE` (`src/database.js:9`) serializes writers across
  *processes*. NetTrello's check is **check-then-act in C# code**. What makes
  it safe anyway, here?

- `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:10` — a
  `SemaphoreSlim(1, 1)` write lock, and
  `InMemoryBoardRepository.cs:67-85` — `UpdateAsync` takes the lock, clones
  the stored board, runs the callback (version check + domain mutation) on the
  clone, and swaps the clone back in. The check and the act are inside the
  same critical section, so within **one process** the race is closed. The
  test `tests/NetTrello.Tests/BoardAggregateTests.cs:90-101`
  (`TwoWritesWithTheSameExpectedVersion_LeaveExactlyOneWinner`) proves exactly
  that and nothing more: run a second API process (or swap in any out-of-process
  store) and nothing enforces the version, because the predicate lives in C#
  memory, not in the data store. gitjira's equivalent test
  (`tests/concurrency.test.js:62`) races two *separate server processes*
  against one database file — NetTrello has no test at that strength, and with
  the in-memory repository it *cannot*: two processes do not even share state.
  Exercises 2.2 and 3.1 move the predicate into storage and then write that
  stronger test.

- Where does the version come from? `src/NetTrello.Api/Domain/Board.cs:167-171`
  — every successful mutation calls `Touch(now)`, which does `Version++`. The
  increment and the mutation are both inside the repository's critical
  section, so they are atomic together — the in-memory analogue of gitjira's
  `version=version+1` inside the UPDATE.

## 4. What the 409 carries — and what it doesn't

- `src/NetTrello.Api/Domain/DomainException.cs:20` — `Conflict(...)` is
  status 409, and `src/NetTrello.Api/Middleware/ExceptionHandlingMiddleware.cs:29`
  serializes it as a problem body with the stable `code`
  `board.version_conflict`.
- gitjira's 409 body carries **the winner's current issue state**
  (`src/app.js:279`), so the losing client can show what won without another
  request. NetTrello's 409 body carries only the error text — the client must
  refetch. Not wrong, but a real contract difference; exercise 3.2 makes you
  catalogue these.

## 5. The losing client destroys the draft

This is where NetTrello's story ends differently from gitjira's.

- `client/src/hooks/useBoard.ts:98-103` — `mutateBoard` catches the error,
  sets a banner message, and on a 409 **refetches the board and replaces
  state**: `setBoard(refreshed)`. The stale write is correctly not retried —
  but nothing preserves what the user typed.
- `client/src/components/CardEditor.tsx:22-36` — `handleSubmit` awaits
  `onUpdate(...)` and then calls `onClose()` **unconditionally**. `mutateBoard`
  swallowed the error, so the promise resolves, the modal closes, the local
  form state (`CardEditor.tsx:15-20`) is unmounted, and the draft is gone. The
  user sees an error banner over a board that shows the winner's values.
- Contrast gitjira `public/dialogs.js:85` — `showConflict()` leaves every form
  field untouched, disables Save so the stale write cannot be resubmitted, and
  makes reloading the winner an explicit user action. Exercise 2.4 builds the
  NetTrello equivalent.

This claim is read-in-source: no automated test covers the client (the repo
has no client test infrastructure — itself a finding), and this tour was
written without running the React app. The code path is short enough to verify
by reading; exercise 1.4 has you confirm it live in two tabs.

## 6. The two invariants with no NetTrello address at all

- **Authorization inside the write.** `src/NetTrello.Api/Program.cs:30-39` —
  the pipeline is exception handling, security headers, CORS, health checks,
  endpoints. No `UseAuthentication`, no `UseAuthorization`, no user identity
  anywhere in the request path. `BoardMember`
  (`src/NetTrello.Api/Domain/BoardMember.cs`) is pure data — an assignee
  dropdown, not a principal. gitjira re-checks membership *inside* the write
  transaction (`src/app.js:265` calling `member()`, `src/app.js:91`) so a
  concurrent revocation cannot slip between check and write; NetTrello has no
  check to place anywhere. Exercise 2.5 sketches the smallest honest version.
- **Activity atomic with mutation.** gitjira inserts the activity event in the
  same transaction as the edit and proves the rollback with a forced-trigger
  test (`tests/api.test.js:295`). NetTrello has no activity/audit concept in
  any file — nothing to make atomic. Exercise 2.3 adds one *inside the
  repository's critical section from day one*, which is the entire point:
  retrofitting atomicity later is the expensive path.

## 7. The comparison table

| Invariant | gitjira-rebuild enforcement | NetTrello enforcement |
|---|---|---|
| Lost-update protection (optimistic concurrency) | CAS in the storage engine: `UPDATE ... WHERE id=? AND version=?` under `BEGIN IMMEDIATE` (`src/app.js:277`, `src/database.js:9`); version is a required request field; proven across two processes (`tests/concurrency.test.js:62`) | **PARTIAL**: check-then-act in `BoardService.MutateAsync` (`BoardService.cs:112-115`) under a single-process `SemaphoreSlim` (`InMemoryBoardRepository.cs:10,67-85`); **opt-in** — a missing `X-Board-Version` header skips the check (`BoardEndpoints.cs:48-51`); proven in-process only (`BoardAggregateTests.cs:90`). Mandatory check: exercise 2.1; storage-level predicate: 2.2; two-process proof: 3.1 |
| Authorization inside the write | Membership re-checked inside the write transaction (`src/app.js:265`, `member()` at `src/app.js:91`) | **NOT ENFORCED (exercise 2.5)** — no authentication or authorization exists (`Program.cs:30-39`) |
| Activity log atomic with mutation | Activity insert in the same transaction; rollback proven by forced trigger (`tests/api.test.js:295`) | **NOT ENFORCED (exercise 2.3)** — no activity log exists |
| Draft-preserving 409 recovery | `showConflict()` keeps the form, disables Save, reload is explicit (`public/dialogs.js:85,151`) | **NOT ENFORCED (exercise 2.4)** — 409 silently replaces board state (`useBoard.ts:100-103`) and the editor closes, discarding the draft (`CardEditor.tsx:35`) |

## 8. What to take across stacks

The lesson is not "SQLite good, in-memory bad." It is that **every invariant
has to live somewhere, and where it lives determines what can break it**.
gitjira pushed the lost-update check into the storage engine, so even a second
process cannot bypass it; NetTrello holds it in application code behind an
optional header, so a missing header — or a second process — bypasses it
silently. When you can say, for each row of the table, *which deployment
change would break NetTrello's current enforcement* (second instance; durable
store without a token; any client that drops the header), you have the
cross-stack lesson. Then go earn it in [EXERCISES.md](EXERCISES.md).
