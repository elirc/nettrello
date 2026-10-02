# Cross-stack comparison course: same promises, different stack

The central question of this course: **a kanban tracker makes the same promises
whether it is built in Express+SQLite or ASP.NET Core — so where does each
promise live in this stack, and which ones does this codebase actually keep?**

This course pairs NetTrello with one specific sibling:
**gitjira-rebuild** (`astrafinalorganize/gitjira-rebuild`), a kanban/issue
tracker in Express+SQLite whose four core invariants — compare-and-swap
version predicate, authorization inside the write transaction, activity atomic
with mutation, draft-preserving 409 recovery — are the measuring stick this
course holds NetTrello against.

## Who this is for

You have finished (or at least read) the existing NetTrello course in
[`../README.md`](../README.md) — the nine chapters on PATCH presence
semantics, aggregates, and the clone-on-write repository — and you have done
gitjira-rebuild's CODE-TOUR and at least exercise 1.1 there. This course does
not repeat either; it sits on top of both and asks the comparison question.

## The honest headline

Against the four gitjira invariants, NetTrello's scorecard (verified against
the source and the 21-test suite on 2026-10-02; details and line anchors in
[CODE-TOUR.md](CODE-TOUR.md)):

1. **Lost-update protection** — partially kept. There is a version check, but
   it is *optional* (omit the header and it is skipped) and it is
   check-then-act under a single-process lock, not a storage-level predicate.
2. **Authorization inside the write** — not kept. There is no authentication
   or authorization anywhere in the API.
3. **Activity atomic with mutation** — not kept. There is no activity log at
   all.
4. **Draft-preserving conflict recovery** — not kept. On a 409 the client
   silently replaces the board with the winner's state and the editor closes,
   destroying the loser's draft.

A missing invariant is not a reason to look away — it is the course content.
Each absence becomes an exercise where you first *demonstrate* the gap with a
real interleaving, then build the missing enforcement.

## Sequence

1. **Prerequisite check.** `dotnet test tests/NetTrello.Tests/NetTrello.Tests.csproj`
   must report 21 passed before you start. If you have not read the existing
   course, at minimum read [`../01-CODEBASE-MAP.md`](../01-CODEBASE-MAP.md)
   and [`../02-CONCEPTS.md`](../02-CONCEPTS.md) (the optimistic-concurrency
   glossary entries) first.
2. **Trace the promise.** Work through [CODE-TOUR.md](CODE-TOUR.md) with the
   files open: one concurrent card-edit story traced through both stacks,
   ending in the four-row comparison table. Predict each layer's behavior
   before reading the answer.
3. **Break and demonstrate.** [EXERCISES.md](EXERCISES.md) Tier 1. Do 1.1
   yourself before reading the completed run in
   [worked-examples/break-the-version-check.md](worked-examples/break-the-version-check.md);
   the model run is for calibrating how you observe and report, not for
   spoiling the prediction.
4. **Build the missing invariants.** Tier 2: make the version check mandatory,
   move the predicate into storage, add an atomic activity log, make the
   client keep the draft.
5. **Move the boundary.** Tier 3: the two-process race in the style of
   gitjira's `tests/concurrency.test.js`, and the contract-compatibility port
   of gitjira's API assertions.

## Relationship to the other learning material in this repo

- [`../README.md`](../README.md) (the nine-chapter astraupskill course) owns
  PATCH semantics, presence vs. null, aggregate validation, and the test
  harness history. Start there if you are new to the repo.
- [`../../docs/upskill/`](../../docs/upskill/README.md) is the broad
  career-track lab (cartography, reading gym, tickets). It predates the test
  suite and some of its "missing pieces" claims are stale; trust `dotnet test`
  over any document, including this one.
- This course owns exactly one thing: the cross-stack invariant comparison
  with gitjira-rebuild, and the exercises that close the gaps it exposes.

## Evidence discipline

Claims in this course come in three strengths, and the text says which is
which: **ran here** (command and real output, recorded in the worked example),
**read in source** (file:line anchor you can open), and **exercise** (does not
exist yet; proposed work). Nothing below is a claim about production behavior
— the repository is in-memory and single-process, and several conclusions
depend on exactly that.
