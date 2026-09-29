using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace NetTrello.Tests;

/// <summary>
/// HTTP-level checks that replace tests/http-regression.ps1. WebApplicationFactory boots the real
/// Program.cs in-process: no port, no Start-Process, no readiness polling, no PowerShell and no
/// DOTNET_ROLL_FORWARD. These prove the *serialized* contract - the status code and the `code`
/// field of the problem body - which a service-level test cannot.
/// </summary>
public sealed class BoardApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly Guid SeededBoardId = Guid.Parse("78e3d235-44e3-4668-aa9b-81583d4a5fd5");

    private readonly WebApplicationFactory<Program> _factory;

    public BoardApiTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private static HttpContent Json(string body) => new StringContent(body, Encoding.UTF8, "application/json");

    private async Task<(HttpClient Client, JsonElement Card)> ArrangeAsync()
    {
        var client = _factory.CreateClient();
        var board = await client.GetFromJsonAsync<JsonElement>($"/api/boards/{SeededBoardId}");
        var card = board.GetProperty("columns")[0].GetProperty("cards")[0].Clone();
        return (client, card);
    }

    [Fact]
    public async Task TitleOnlyPatch_Returns200_AndPreservesTheOmittedDescription()
    {
        var (client, card) = await ArrangeAsync();
        var cardId = card.GetProperty("id").GetString();
        var originalDescription = card.GetProperty("description").GetString();

        using var response = await client.PatchAsync(
            $"/api/boards/{SeededBoardId}/cards/{cardId}",
            Json("{\"title\":\"HTTP title only\"}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var updated = body.GetProperty("columns")[0].GetProperty("cards")
            .EnumerateArray().Single(item => item.GetProperty("id").GetString() == cardId);
        Assert.Equal("HTTP title only", updated.GetProperty("title").GetString());
        Assert.Equal(originalDescription, updated.GetProperty("description").GetString());
    }

    [Theory]
    [InlineData("{\"priority\":\"Imaginary\"}", "card.priority_invalid")]
    [InlineData("{}", "card.patch_empty")]
    public async Task MalformedPatch_Returns400_WithItsContractCode(string body, string expectedCode)
    {
        var (client, card) = await ArrangeAsync();
        var cardId = card.GetProperty("id").GetString();

        using var response = await client.PatchAsync($"/api/boards/{SeededBoardId}/cards/{cardId}", Json(body));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(expectedCode, problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Health_IsReachable()
    {
        var client = _factory.CreateClient();

        using var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
