# Testing and debugging

## Start with `dotnet test`

The suite lives in `tests/NetTrello.Tests` and runs with one command:

```powershell
dotnet test tests/NetTrello.Tests/NetTrello.Tests.csproj -c Release
```

It reports **21 cases** across three classes:

| File | What it covers |
|---|---|
| `BoardAggregateTests.cs` | Rejected `MoveCard` (missing target, full target, negative position), same-column reorder at capacity, late label validation, stale `expectedVersion`, two competing writes, repository clone isolation |
| `CardPatchTests.cs` | Title-only PATCH, explicit-null clears, the full UI-shaped update, a `[Theory]` of five malformed bodies, and the serializer's unknown-property rejection |
| `BoardApiTests.cs` | The same PATCH contract over real HTTP, asserting status codes and the serialized problem `code` |

`BoardFixture.cs` holds the arrangement: a fixed clock at 2026-09-12 12:00Z, a board with an unlimited "Source" column holding two cards and a capacity-two "Target" column that is already full, one member, and an in-memory repository. **Each test constructs its own fixture**, so no test inherits state from another - that is the first thing a real test framework buys you over a single script.

A representative case:

```csharp
[Fact]
public void MoveCard_ToAFullColumn_DoesNotRemoveTheCardFromItsSource()
{
    var fixture = new BoardFixture();
    var before = BoardFixture.Describe(fixture.Board);

    var error = Assert.Throws<DomainException>(() =>
        fixture.Board.MoveCard(fixture.First.Id, fixture.TargetColumnId, 0, BoardFixture.Now));

    Assert.Equal("column.card_limit", error.Code);
    Assert.Equal(before, BoardFixture.Describe(fixture.Board));
}
```

`Describe` serialises the **whole board** - `Version`, `UpdatedAt`, every column and every card field - to a JSON string, and the test compares the string taken before the rejected call with the one taken after. Comparing independent serialisations, rather than two references to the same object, is what makes "nothing was mutated" a real assertion. Including `Version` and `UpdatedAt` matters: an implementation that validated correctly but still called `Touch()` would look fine to a columns-and-cards-only snapshot.

The malformed bodies are a `[Theory]`, which is xUnit's table-driven test: one method, five rows, five independently reported results.

```csharp
[Theory]
[InlineData("{\"priority\":\"Unknown\"}", "card.priority_invalid")]
[InlineData("{\"title\":null}", "card.field_invalid")]
[InlineData("{\"dueDate\":42}", "card.field_invalid")]
[InlineData("{\"labels\":[\"ok\",4]}", "card.labels_invalid")]
[InlineData("{}", "card.patch_empty")]
public async Task MalformedPatch_IsRejectedWithItsContractCode_AndChangesNothing(string json, string expectedCode)
```

## Why this replaced a console app and a PowerShell script

The delivered version had `tests/NetTrello.Checks/Program.cs`, a console executable whose `Expect*` helpers threw on the first mismatch, and `tests/http-regression.ps1`, which started the API with `Start-Process`, polled `/health` 30 times and killed the process in `finally`. Both are still readable in the repository history and both worked - but:

- **Fail-fast masking.** The console app stopped at the first failure, so you learned about one broken check per run and had no idea whether the other twenty were still good. A test runner reports all 21 independently.
- **No real count.** "The harness passed" is one result. `Passed: 21` is a number you can watch go up.
- **Shared state.** Every check in the console app mutated the same board in sequence, so check 12 depended on what check 3 did. Each `[Fact]` now starts from a fresh `BoardFixture`.
- **Environment coupling.** The PowerShell script needed PowerShell, a free TCP port, a readiness loop and `DOTNET_ROLL_FORWARD=Major`. `WebApplicationFactory<Program>` needs none of those - it boots `Program.cs` in-process over an in-memory transport:

```csharp
public sealed class BoardApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public BoardApiTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Theory]
    [InlineData("{\"priority\":\"Imaginary\"}", "card.priority_invalid")]
    [InlineData("{}", "card.patch_empty")]
    public async Task MalformedPatch_Returns400_WithItsContractCode(string body, string expectedCode)
    {
        var (client, card) = await ArrangeAsync();
        using var response = await client.PatchAsync(
            $"/api/boards/{SeededBoardId}/cards/{card.GetProperty("id").GetString()}", Json(body));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(expectedCode, problem.GetProperty("code").GetString());
    }
}
```

Two setup requirements are easy to forget on a new project: the API's `Program.cs` must end with `public partial class Program { }` (a top-level-statements program otherwise has no accessible entry-point type), and the test project needs `Microsoft.AspNetCore.Mvc.Testing` plus a `ProjectReference` to the API.

The HTTP layer is worth testing separately even though the service is already covered: only here do you see the serialized `code` field and the status code. That catches middleware regressions where a `DomainException` turns into an HTML error page, or where the machine-readable code disappears from the body. Because the repository is in memory, each factory gives a clean seeded board - a fixture isolation property, not evidence of durable storage.

## Debugging: identify the layer first

When a test fails, read the error code before the stack trace. A `card.patch_empty` means binding reached the DTO but no JSON property was present. A `card.priority_invalid` means the body was present but parsing rejected it. A `column.card_limit` failure whose snapshot comparison *also* fails means validation happened after removal - the original bug. A second successful expected-version write means the version check escaped the repository lock. A clone-isolation failure means the repository handed out a live aggregate.

The strongest assertions are independent and positive. `Assert.Equal(2, card.Labels.Count)` proves the update returned a nonempty label result, where an `All` over an empty list would pass accidentally. The competing-write test counts exactly one `true` and one `false`, proving both branches ran rather than that neither did.
