# .nettrello upskill course

This nine-chapter course teaches the actual .nettrello stack: ASP.NET Core minimal APIs, domain aggregates, an in-memory clone-on-write repository, explicit clocks, JSON PATCH presence semantics, and optimistic board versions. Start with the map, then study the distinctions between omitted values and explicit nulls, read the worked source change, run the domain/service harness, complete the exercises, compare solutions, and trace an HTTP request. The verification chapter records the reproducible commands.

The bounded improvement has two goals. First, a PATCH must preserve omitted fields, clear only explicitly nullable fields, reject unknown enum values, and reject an empty body before touching the board. Second, aggregate operations must validate destination, position, capacity, and labels before mutating. The repository's semaphore and clone behavior remains a useful safety boundary; the course explains exactly what it proves and what it cannot prove.

Read the [API source](../src/NetTrello.Api) and the [xUnit suite](../tests/NetTrello.Tests) alongside the chapters; `dotnet test` is the command that proves every claim here. (The delivered version used a console harness at `../tests/NetTrello.Checks/Program.cs` and a PowerShell script at `../tests/http-regression.ps1`; both are still present and still work, and chapter 04 explains what the conversion bought. The original [verification record](evidence/verify-03.json) is the evidence for that earlier run.) All examples are local and use synthetic or seeded board data. No real database, credentials, external service, or browser session is required.


Each chapter names a real file so the learner can move between explanation and source. The examples use C# syntax from this repository and mark shortened excerpts; the executable truth remains in the harness and API. The course's central review question is always "what state is observable after rejection?" For a domain method, inspect the same aggregate; for a repository operation, fetch a new clone; for HTTP, inspect both status and body. Keeping those observation points separate prevents an apparently green assertion from proving the wrong boundary.


The course is intended for a junior engineer who can run commands locally but has not seen this repository before. It explains why the source changes are bounded, how to reproduce them, and which claims remain out of scope. Read the small domain examples first, then use the larger client and middleware files as context. The goal is a reliable mental model of state transitions rather than memorizing framework syntax.

## Prerequisites and glossary

**Prerequisites.** You should be able to read C# classes and `async`/`await`. No prior ASP.NET Core, DDD or xUnit experience is assumed; chapters 02 and 04 introduce what is used. You do need to know what an HTTP `PATCH` is meant to mean, which is the subject of the whole course. If you have not met ASP.NET Core minimal APIs before, the `.netvg` course in this workspace is a smaller starting point.

**What you need installed.**

| Tool | Version used | Unlocks |
|---|---|---|
| .NET SDK 10.x (10.0.302 here) | the API targets `net8.0`; the test project targets `net10.0` | `dotnet build` and `dotnet test` (21 cases, 4 of them over HTTP). No `DOTNET_ROLL_FORWARD` override is needed for the tests. |
| Node 20+ and npm (optional) | | building the React client under `client/`. The client is **not** exercised by any test and is optional for this course. |

No database server, no Docker, no Python, no PowerShell.

**Time estimate (about 4-6 hours, one or two sittings).**

| Chapter | Estimate |
|---|---|
| 01 Codebase map | 20 min |
| 02 Concepts (incl. the `JsonValueKind` table and the database section) | 60 min |
| 03 Worked change | 40 min |
| 04 Testing and debugging | 45 min |
| 05 Practice (9 exercises, each with a Check, + red-first lab) | 90 min |
| 06 Solutions and review | 30 min |
| 07 Trace lab | 30 min |
| VERIFICATION | 10 min |

**Glossary.**

| Term | What it means here | Where in this project |
|---|---|---|
| **PATCH vs PUT** | `PUT` replaces the whole resource with the body you send; `PATCH` changes only the parts you mention. Treating a PATCH body like a PUT DTO is how "field omitted" silently becomes "field cleared". | `UpdateCardRequest` in `src/NetTrello.Api/Contracts/BoardDtos.cs` |
| **Aggregate** | The object graph you load, change and save as one unit: `Board` owns its columns, which own their cards. You never edit a card directly; you call a method on the board. | `Board` owning columns and cards, `src/NetTrello.Api/Domain/Board.cs` |
| **`JsonElement` / `JsonValueKind`** | `System.Text.Json`'s untyped view of one JSON value. A non-nullable `JsonElement` property is `Undefined` when the client omitted the field, which is the distinction the whole course rests on. | the `JsonElement` properties of `UpdateCardRequest`, `Contracts/BoardDtos.cs` |
| **`[JsonUnmappedMemberHandling(Disallow)]`** | Makes a typo'd or unknown JSON property a deserialization error (a 400) instead of a silently ignored no-op. | the attribute on `UpdateCardRequest`, `Contracts/BoardDtos.cs` |
| **`Enum.TryParse` + `Enum.IsDefined`** | `TryParse` alone accepts `"99"` and produces an undefined enum value; `IsDefined` is what rejects it. You need both. | the priority reader inside `ResolveAgainst`, `Contracts/BoardDtos.cs` |
| **Optimistic concurrency / `expectedVersion`** | Two clients edit the same board; instead of locking, each write states the version it read. A mismatch is rejected (`board.version_conflict` → 409; chapter 02 explains why an EF Core version would more often use 412) rather than silently overwriting. Sending the header is optional; see 06, finding A. | `MutateAsync` raising `board.version_conflict` in `Application/BoardService.cs` |
| **Clone-on-write repository** | `GetAsync` returns a deep copy, so a caller who mutates it cannot corrupt stored state. See chapter 02 for what EF Core's change tracker does instead. | `Clone()` in `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs` |
| **Problem response / contract `code`** | The machine-readable error body. Tests assert the `code` field, not just a non-200 status, so a middleware change that turns an error into an HTML page is caught. | `WriteProblemAsync` in `Middleware/ExceptionHandlingMiddleware.cs` |
| **Snapshot assertion** | Serialising the whole board before and after a rejected operation and comparing the strings, which is how "nothing was mutated" becomes a real assertion. | `tests/NetTrello.Tests/BoardAggregateTests.cs` |
| **`[Fact]` / `[Theory]` / `[InlineData]`** | xUnit's single test, table-driven test and one row of that table. | the `[InlineData]` rows in `tests/NetTrello.Tests/CardPatchTests.cs` |
| **`WebApplicationFactory<Program>`** | The standard .NET tool that boots the real API in-process for tests and hands you an `HttpClient`. No port, no child process. | `tests/NetTrello.Tests/BoardApiTests.cs` |
| **Reindexing positions** | After inserting or removing a card, the remaining cards' `Position` values must be renumbered so the order stays contiguous. | `RepositionCard` in `src/NetTrello.Api/Domain/BoardColumn.cs` |

Read in order: [01-CODEBASE-MAP](01-CODEBASE-MAP.md) | [02-CONCEPTS](02-CONCEPTS.md) | [03-WORKED-CHANGE](03-WORKED-CHANGE.md) | [04-TESTING-AND-DEBUGGING](04-TESTING-AND-DEBUGGING.md) | [05-PRACTICE](05-PRACTICE.md) | [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md) | [07-TRACE-LAB](07-TRACE-LAB.md) | [VERIFICATION](VERIFICATION.md)
