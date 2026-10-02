# Worked example: break the version check (Exercise 1.1)

[EXERCISES.md](../EXERCISES.md) exercise 1.1 performed once, for real, on this
machine (Windows 11, .NET SDK 10.0.302, 2026-10-02), with the actual output
captured. The loop is the same one gitjira-rebuild's
`break-the-cas.md` models: **predict → break → observe → revert**. Do your own
run before reading past the spoiler break.

## Predict first

Remove the version-conflict check from `MutateAsync` in
`src/NetTrello.Api/Application/BoardService.cs`. Before touching anything,
write down:

1. Which of the 21 tests fail?
2. Deterministic or flaky — will `TwoWritesWithTheSameExpectedVersion_LeaveExactlyOneWinner`
   fail every run?
3. gitjira's exercise 1.1 removed `AND version=?` from a SQL UPDATE. What is
   the *structural* difference between that break and this one?

---

**Spoiler break.** Everything below is the answer key.

---

## Baseline

```
dotnet test tests/NetTrello.Tests/NetTrello.Tests.csproj
```

```
Passed!  - Failed:     0, Passed:    21, Skipped:     0, Total:    21, Duration: 8 s - NetTrello.Tests.dll (net10.0)
```

## The break

In `BoardService.MutateAsync`, the repository callback loses its guard:

```diff
         var updated = await _boards.UpdateAsync(boardId, board =>
         {
-            if (expectedVersion is not null && board.Version != expectedVersion.Value)
-            {
-                throw DomainException.Conflict("board.version_conflict", "The board changed since it was last loaded. Refresh and try again.");
-            }
-
             return update(board);
         }, cancellationToken);
```

Everything else survives: the header is still parsed and validated
(`BoardEndpoints.cs:46-59` — a malformed header is still a 400), the version
still increments on every write (`Board.cs:170`), the 409 machinery in the
middleware is still wired. The ceremony is intact; only the one place where
the expectation was *compared to reality* is gone — the exact anatomy of
gitjira's break, one layer higher in the stack.

## What actually happened

```
dotnet test tests/NetTrello.Tests/NetTrello.Tests.csproj
```

```
  Failed NetTrello.Tests.BoardAggregateTests.StaleExpectedVersion_IsRejected [85 ms]
  Error Message:
   Assert.Throws() Failure: No exception was thrown
  Failed NetTrello.Tests.BoardAggregateTests.TwoWritesWithTheSameExpectedVersion_LeaveExactlyOneWinner [49 ms]
  Error Message:
   Assert.Equal() Failure: Values differ
Failed!  - Failed:     2, Passed:    19, Skipped:     0, Total:    21, Duration: 8 s - NetTrello.Tests.dll (net10.0)
```

The race test's detail, from the same broken build:

```
  Failed NetTrello.Tests.BoardAggregateTests.TwoWritesWithTheSameExpectedVersion_LeaveExactlyOneWinner [185 ms]
  Error Message:
   Assert.Equal() Failure: Values differ
Expected: 1
Actual:   2
```

`Expected: 1, Actual: 2` — **both** writers that read the same version
reported success. Two columns named "one" and "two" now exist where exactly
one should; the second write silently built on a board it had never seen.
That is the lost update, one abstraction level up from gitjira's
`[200, 200]` instead of `[200, 409]`.

**Determinism.** I ran the broken suite twice; both runs failed the same two
tests the same way. Same signature as gitjira's break and for the same
reason: the test counts *outcomes* (one success, one `DomainException`), and
with the check gone a conflict can never be raised in any interleaving — the
failure is structural, not timing-dependent. Contrast exercise 1.3 (remove the
`SemaphoreSlim` instead), where the guard still exists but its atomicity is
gone, and a green run proves nothing.

**What stayed green matters too.** All 19 others passed, including every
PATCH-semantics test and `TheRepositoryHandsOutAnIsolatedClone`. The
clone-on-write isolation and the presence/null handling are genuinely separate
guarantees owned by other code — the suite just told you the module
boundaries are real.

## The three answers

1. Exactly two tests fail, both in `BoardAggregateTests.cs` (lines 78 and 90)
   — the only two that assert the concurrency invariant.
2. Deterministic, both runs — see above.
3. The structural difference: gitjira's predicate lived in the *storage
   engine*, so deleting it changed a SQL statement's meaning; NetTrello's
   lives in *application code*, so deleting it is just removing an `if`. The
   symmetry is the lesson — and so is the asymmetry: in NetTrello, a client
   can reach this same broken behavior **without any code change at all**, by
   omitting the `X-Board-Version` header (`BoardEndpoints.cs:48-51`). The
   break you just made by hand ships, today, as an opt-out. That is exercise
   1.2.

## Restore and confirm

```
git checkout -- src/NetTrello.Api/Application/BoardService.cs
dotnet test tests/NetTrello.Tests/NetTrello.Tests.csproj
```

```
Passed!  - Failed:     0, Passed:    21, Skipped:     0, Total:    21, Duration: 8 s - NetTrello.Tests.dll (net10.0)
```

`git status --short` prints nothing. Never leave the break in place: every
exercise ends with a green run and a clean tree, or it is not finished.
