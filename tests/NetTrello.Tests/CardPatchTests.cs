using NetTrello.Api.Contracts;
using NetTrello.Api.Domain;
using System.Text.Json;
using Xunit;

namespace NetTrello.Tests;

/// <summary>
/// PATCH presence semantics: omitted (JsonValueKind.Undefined) keeps the current value, an
/// explicit null clears a nullable one, a bad value is a 400 and nothing is mutated.
/// One [Fact] per behaviour: unlike the old fail-fast console harness, a failure here does not
/// hide the checks that come after it.
/// </summary>
public sealed class CardPatchTests
{
    [Fact]
    public async Task TitleOnlyPatch_AppliesTitleAndPreservesEveryOmittedField()
    {
        var fixture = new BoardFixture();

        var updated = await fixture.Service.UpdateCardAsync(
            fixture.BoardId, fixture.First.Id, BoardFixture.Request("{\"title\":\"Updated title\"}"), null, CancellationToken.None);

        var card = updated.Columns.SelectMany(column => column.Cards).Single(c => c.Id == fixture.First.Id);
        Assert.Equal("Updated title", card.Title);
        Assert.Equal("Keep me", card.Description);
        Assert.Equal(CardPriority.High, card.Priority);
        Assert.Equal(2, card.Labels.Count);
    }

    [Fact]
    public async Task ExplicitNulls_ClearNullableFields_AndAStringPriorityIsParsed()
    {
        var fixture = new BoardFixture();

        var updated = await fixture.Service.UpdateCardAsync(
            fixture.BoardId,
            fixture.First.Id,
            BoardFixture.Request("{\"description\":null,\"assigneeId\":null,\"dueDate\":null,\"labels\":null,\"priority\":\"Low\"}"),
            null,
            CancellationToken.None);

        var card = updated.Columns.SelectMany(column => column.Cards).Single(c => c.Id == fixture.First.Id);
        Assert.Equal(string.Empty, card.Description);
        Assert.Null(card.AssigneeId);
        Assert.Null(card.DueDate);
        Assert.Empty(card.Labels);
        Assert.Equal(CardPriority.Low, card.Priority);
    }

    [Fact]
    public async Task CompleteUpdateFromTheUi_RemainsCompatible()
    {
        var fixture = new BoardFixture();
        var request = new UpdateCardRequest
        {
            Title = BoardFixture.Element("Full update"),
            Description = BoardFixture.Element("All fields"),
            Priority = BoardFixture.Element("Critical"),
            AssigneeId = BoardFixture.Element(fixture.Member.Id.ToString()),
            DueDate = BoardFixture.Element(BoardFixture.Now.AddDays(5).ToString("O")),
            Labels = BoardFixture.ElementArray("alpha", "beta")
        };

        var updated = await fixture.Service.UpdateCardAsync(fixture.BoardId, fixture.First.Id, request, null, CancellationToken.None);

        var card = updated.Columns.SelectMany(column => column.Cards).Single(c => c.Id == fixture.First.Id);
        Assert.Equal("Full update", card.Title);
        Assert.Equal(2, card.Labels.Count);
    }

    // The malformed bodies the old harness checked one after another, as data rows.
    [Theory]
    [InlineData("{\"priority\":\"Unknown\"}", "card.priority_invalid")]
    [InlineData("{\"title\":null}", "card.field_invalid")]
    [InlineData("{\"dueDate\":42}", "card.field_invalid")]
    [InlineData("{\"labels\":[\"ok\",4]}", "card.labels_invalid")]
    [InlineData("{}", "card.patch_empty")]
    public async Task MalformedPatch_IsRejectedWithItsContractCode_AndChangesNothing(string json, string expectedCode)
    {
        var fixture = new BoardFixture();
        var before = BoardFixture.Describe(await fixture.ReloadAsync());

        var error = await Assert.ThrowsAsync<DomainException>(() => fixture.Service.UpdateCardAsync(
            fixture.BoardId, fixture.First.Id, BoardFixture.Request(json), null, CancellationToken.None));

        Assert.Equal(expectedCode, error.Code);
        Assert.Equal(before, BoardFixture.Describe(await fixture.ReloadAsync()));
    }

    [Fact]
    public void UnknownProperty_IsRejectedByTheSerializer_NotByTheService()
    {
        // [JsonUnmappedMemberHandling(Disallow)] means a typo'd field never reaches the service:
        // deserialization itself throws, which the API maps to a 400.
        Assert.Throws<JsonException>(() => BoardFixture.Request("{\"unknownField\":true}"));
    }
}
