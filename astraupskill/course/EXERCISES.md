# Exercises: demonstrate the gap, then close it

Ordered by tier. Write your prediction down before each one; the learning is
in comparing it with what happened. Work on a branch and never commit a break.

Baseline before and after every exercise:

```powershell
dotnet test tests/NetTrello.Tests/NetTrello.Tests.csproj
```

Must report **Passed: 21** (as it did on 2026-10-02 on this machine) before
you start and after you finish. On a memory-constrained machine run one
`dotnet` command at a time.

Where an invariant exists, Tier 1 breaks it and watches which test speaks.
Where an invariant is **missing** (table rows 2-4 in
[CODE-TOUR.md](CODE-TOUR.md) §7), the Tier 1 exercise is instead to
*demonstrate the bug* — write the interleaving that loses data, as a test
where feasible — and the Tier 2 exercise builds the enforcement.

---

## Tier 1 — Break and observe / demonstrate the bug (half a day each)

**1.1 Break the version check.** In
`src/NetTrello.Api/Application/BoardService.cs` (`MutateAsync`), delete the
`if (expectedVersion is not null && board.Version != expectedVersion.Value)`
block.
*Predict first:* which of the 21 tests fail? Will the concurrent-writers test
(`BoardAggregateTests.cs:90`) fail every run or only sometimes — and how does
that compare with gitjira exercise 1.1, where the equivalent break was
deterministic because the test asserted the status *pair*?
*Done when:* you can name the failing assertions without re-reading the test
file, and explain why `TheRepositoryHandsOutAnIsolatedClone` still passes —
which guarantee belongs to the repository and which to the service.
A completed run with real captured output is in
[worked-examples/break-the-version-check.md](worked-examples/break-the-version-check.md)
— do your own run first.

**1.2 Demonstrate the optional-header lost update.** No production code
changes. Add a test to `tests/NetTrello.Tests/BoardAggregateTests.cs`:

1. Load the board, remember `version`.
2. Writer A: `AddColumnAsync(..., expectedVersion: version, ...)` — succeeds.
3. Writer B: `UpdateCardAsync(..., expectedVersion: null, ...)` with a card
   edit built from the *stale* read — the write a client performs when it
   omits `X-Board-Version` (`BoardEndpoints.cs:48-51`).

*Predict first:* status of B, and the final board state. Then assert what
actually happens: B returns success and its stale field values stand — the
textbook lost update, accepted politely.
*Done when:* the test passes (it documents current behavior — name it
something honest like `OmittedVersion_SilentlyOverwrites_CurrentBehavior`),
and you can say in one sentence why gitjira cannot express this interleaving
at all. Keep the test; exercise 2.1 will flip its assertions.

**1.3 Break the write lock.** In
`src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs`, remove the
`_writeLock.WaitAsync`/`Release` pair in `UpdateAsync` (keep the clone calls).
*Predict first:* does the suite go red? `TwoWritesWithTheSameExpectedVersion_`
`LeaveExactlyOneWinner` races only two writers through a now-unguarded
check-then-act — run the suite five times and record each result.
*Done when:* you have five recorded outcomes and an explanation of whichever
you saw. A suite that stays green with the lock gone is the scarier result
(the gitjira worked example calls this the sometimes-green trap): it means the
race window is just too narrow for two writers to hit reliably, not that the
code is safe. Revert and confirm 21/21.

**1.4 Watch the draft die.** Run the API (`dotnet run --project
src/NetTrello.Api`) and the client (requires `npm install` in `client/` —
skip on a memory-constrained machine and do the code-reading half only). Open
the board in two tabs, edit the same card in both, save tab A, then save tab B.
*Predict first:* exactly what tab B shows afterward, from
`useBoard.ts:98-103` and `CardEditor.tsx:22-36` alone.
*Done when:* you can narrate the destruction in file:line order — 409 thrown
by `MutateAsync`, caught in `mutateBoard`, board replaced, promise resolves,
`onClose()` unmounts the form state — and say which single line (the
unconditional `onClose()` at `CardEditor.tsx:35`) gitjira's `showConflict()`
design refuses to have.

## Tier 2 — Build the missing invariant (1-3 days each)

Write the failing test first, in the style of the existing suite: domain/service
assertions in `BoardAggregateTests.cs`, serialized-contract assertions in
`BoardApiTests.cs` via `WebApplicationFactory`.

**2.1 Make the version check mandatory.** Mutating endpoints currently treat a
missing `X-Board-Version` as "don't check". Change `ReadExpectedVersion` /
`MutateAsync` so a missing header on mutations is a 400 with a new stable code
(`board.version_required`). Decide and document the one defensible exception
(does `POST /api/boards` need a version? do column/card *creates*?).
*Done when:* exercise 1.2's test is flipped to assert the rejection; a new
`BoardApiTests` case proves the 400 code over HTTP; the existing header-less
PATCH tests in `BoardApiTests.cs` are updated **deliberately** (not deleted in
confusion — they were documenting the old contract); the client still works
because `useBoard.ts` already sends the version on every mutation; 21+ green.

**2.2 Move the predicate into storage.** Replace (or add alongside)
`InMemoryBoardRepository` with an EF Core + SQLite implementation of
`IBoardRepository` in which `Board.Version` is a real concurrency token
(`IsConcurrencyToken()` / `IsRowVersion()` in the model builder), and map
`DbUpdateConcurrencyException` to the existing
`DomainException.Conflict("board.version_conflict", ...)`.
This is the EF Core answer to the course's central question: gitjira wrote the
predicate by hand in SQL; EF Core writes `WHERE ... AND Version = @original`
for you — but only if you declare the token.
*Done when:* `StaleExpectedVersion_IsRejected` and
`TwoWritesWithTheSameExpectedVersion_LeaveExactlyOneWinner` pass against the
new repository with the `SemaphoreSlim` **removed** — the database predicate,
not a C# lock, must be what makes them pass. Keep the API project green and
do not break the in-memory default for the demo. (Memory note: SQLite in-proc,
no server, no Docker.)

**2.3 Activity log atomic with mutation.** Add an `ActivityEntry` (board id,
kind, summary, timestamp) recorded for every successful mutation, exposed at
`GET /api/boards/{id}/activity`, and written **atomically with the mutation**:
inside `UpdateAsync`'s critical section for the in-memory repository, inside
the same `SaveChanges`/transaction for the 2.2 repository.
*Done when:* a test forces the mutation callback to throw *after* the domain
change (a test-only hook or a poisoned update function) and proves no activity
entry survives; a second test proves a rejected stale write records nothing —
the NetTrello equivalent of gitjira's forced-trigger rollback test
(`tests/api.test.js:295`).

**2.4 Draft-preserving client recovery.** Change the client so a 409 is a
first-class state, not a banner: `mutateBoard` (or a reworked `updateCard`
path) must surface the conflict to `CardEditor`, which keeps the modal open
with the user's values intact, shows the winner's current values beside them,
disables blind re-save, and offers an explicit "discard my draft and load
theirs" action — gitjira's `showConflict()`/`reloadIssue()` pair
(`public/dialogs.js:85,151`) is the model.
*Done when:* the unconditional `onClose()` after `onUpdate` in
`CardEditor.tsx` is gone; the two-tab script from exercise 1.4 ends with tab
B's draft on screen; `npx tsc --noEmit` passes in `client/` (there is no
client test runner — adding even one such test is bonus work worth doing).

**2.5 The smallest honest authorization.** Full auth is a project, not an
exercise; do the invariant-shaped core only. Add an `X-Member-Id` header
(demo-grade identity, documented as such), require the id to be a member of
the board for every mutation, and run the check **inside**
`UpdateAsync`'s critical section — so a concurrent member removal cannot slip
between check and write, which is the entire gitjira lesson
(`src/app.js:265`).
*Done when:* a test removes the member and races the removal against the
member's own write with the same expected version, and exactly one of the two
outcomes (member gone, or write accepted) is ever observable — never both;
and you can say where this check must move when real authentication (the
README's "next production steps") arrives.

## Tier 3 — Move a boundary (1-2 weeks each)

One page of design first: two alternatives, chosen tradeoffs, what evidence
would change your mind. Prerequisite for both: exercise 2.2.

**3.1 The two-process race, in C#.** Port the idea of gitjira's
`tests/concurrency.test.js:62` to this stack: a test that launches **two
separate API processes** (`dotnet run` or published binaries on two ports)
against one shared SQLite file, sends the same stale-version mutation to both,
and asserts exactly one 200, one 409 with `board.version_conflict`, and a
final state that survives restarting both processes. `WebApplicationFactory`
is explicitly not allowed here — the point is crossing the process boundary
the current suite never crosses.
*Done when:* the test is deterministic across five runs (run sequentially —
two processes, not two parallel builds), and you can state precisely which
mechanism replaced the `SemaphoreSlim`: the EF Core concurrency token's
`WHERE` predicate evaluated inside SQLite, the same engine-level enforcement
gitjira wrote by hand.

**3.2 Contract-compatibility port.** Take gitjira's stale-edit API assertions
(`gitjira-rebuild/tests/api.test.js` — the stale PATCH block around line 199)
and port their *meaning* against NetTrello's API as xUnit facts: stale write
→ 409; conflict body machine-readable; no side effects from the rejected
write (board snapshot unchanged, and after 2.3: no activity entry).
Where the contracts genuinely differ, record the difference instead of forcing
it: gitjira's 409 body carries the winner's full current state so the client
needs no second request; NetTrello's problem body carries only `code` and
text. Decide — as a one-page ADR — whether NetTrello should adopt the
state-carrying 409, and implement your decision.
*Done when:* the ported facts run green in `tests/NetTrello.Tests/`, the ADR
names what each contract choice costs the client (an extra GET vs. a fatter
error body and a second serialization path), and the comparison table in
[CODE-TOUR.md](CODE-TOUR.md) is updated to your new reality.

---

## Calibration

You have the cross-stack lesson when you can, from memory: (1) name the four
invariants and, for *both* stacks, the exact mechanism and file that enforces
each; (2) say which deployment change silently disables each NetTrello
enforcement as shipped (second process; omitted header; any durable store
without a token); (3) state evidence at its real strength — "21 in-process
tests pass" is not "lost updates are impossible", and after 3.1 you can say
exactly how much more it became.
