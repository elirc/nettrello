# Verification

Run from this project root. The API targets .NET 8 and the check executable targets .NET 10; install the .NET 10 SDK. The HTTP script explicitly permits the API to run on the installed .NET 10 runtime:

```powershell
dotnet test tests/NetTrello.Tests/NetTrello.Tests.csproj -c Release
```

Observed on 2026-09-17 with SDK 10.0.302 and no roll-forward override:

```
Passed!  - Failed:     0, Passed:    21, Skipped:     0, Total:    21, Duration: 5 s - NetTrello.Tests.dll (net10.0)
```

That is 8 aggregate/concurrency cases, 9 PATCH cases (including a five-row `[Theory]`) and 4 HTTP cases run in-process through `WebApplicationFactory<Program>`. The test project targets `net10.0` while the API stays on `net8.0`, which is why no `DOTNET_ROLL_FORWARD=Major` is needed any more.

The delivered console harness and PowerShell script still exist and still pass; run them if you want to see the earlier shape:

```powershell
dotnet restore NetTrello.sln --ignore-failed-sources
dotnet build NetTrello.sln --no-restore
dotnet run --project tests/NetTrello.Checks/NetTrello.Checks.csproj --no-build
powershell -NoProfile -ExecutionPolicy Bypass -File tests/http-regression.ps1
```

The suite covers four responsibilities: direct domain validation, service PATCH resolution, repository version serialization, and clone isolation. Each rejected `MoveCard` is now followed by a whole-board snapshot comparison that includes `Version` and `UpdatedAt`, closing the gap the delivery admitted (the old snapshot covered columns and cards only). The HTTP script checks serialized status/problem bodies and omitted-field preservation against the actual endpoint. The captured [local verify-03.json](evidence/verify-03.json) record shows restore, build, harness, and HTTP commands all completed with exit code 0; its paired log contains the successful output. It must not be inferred from source inspection.

The exact pre-change files are at the project root under `snapshots/` (i.e. `../snapshots/` from this chapter): `Card-before.cs.txt`, `BoardColumn-before.cs.txt`, `Board-before.cs.txt`, `BoardDtos-before.cs.txt`, `BoardService-before.cs.txt`, `NetTrello.sln-before.txt`, and `README-before.md.txt`. Diff each one against the file it replaced to see exactly what changed — chapter 05's red-first lab walks you through restoring them and watching the suite fail. The nine chapter files are source-grounded and use synthetic or seeded local data only.

Limits: `InMemoryBoardRepository` serializes one process only; it is not a database transaction. Direct domain prevalidation prevents partial aggregate mutation, while the repository clone protects stored state after a callback throws. The HTTP check uses the installed .NET 10 runtime's major roll-forward to execute the net8 API. It does not prove deployment behavior, browser accessibility, durable storage, multi-process concurrency, or production identity.


The recorded evidence should include the SDK version (`dotnet --info` or the runner environment), restore result, build result, harness output, and HTTP output. If the machine has only .NET 10 installed, the net8 API can be launched with the documented major roll-forward setting; do not silently call that a net8 runtime test. If a future environment has .NET 8, remove the roll-forward override and repeat. A failed restore or missing runtime is an environment result and must remain visible in the handoff rather than being replaced with a fabricated pass.


`bin` and `obj` are build products, not source: `.gitignore` already excludes them, and you should never commit them or include them when comparing this tree against the snapshots. When you compare a file with its `../snapshots/*-before.cs.txt` copy, use `git diff -w` or `diff --strip-trailing-cr` — whole-file line-ending and formatting churn otherwise hides a four-line change.

## Fix-pass re-run (2026-09-17)

- Added `tests/NetTrello.Tests` (xUnit + `Microsoft.AspNetCore.Mvc.Testing`), converting every `Expect*` group of the console harness into `[Fact]`/`[Theory]` methods and replacing `tests/http-regression.ps1` with `WebApplicationFactory<Program>` tests. The console harness and the script were **kept**, not deleted.
- `BoardFixture.Describe` now includes `Version` and `UpdatedAt`.
- One prediction was wrong and the run corrected it: `{"labels":["ok",4]}` returns the contract code `card.labels_invalid`, not `card.field_invalid`. The first run reported `Failed: 1, Passed: 20`; the expectation was corrected to the code the service actually returns and the suite is green at 21.

Still not covered by execution: the React client under `client/` has no test runner (no vitest, no Testing Library, and `package.json` has no test script), so `CardEditor` and `boardClient.updateCard` remain source-reviewed only. The claim that the client sends a compatible PATCH body is verified indirectly by `CompleteUpdateFromTheUi_RemainsCompatible`, which sends the shape the client builds — that is not the same as executing the client.
