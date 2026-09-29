using NetTrello.Api.Contracts;
using NetTrello.Api.Domain;
using Xunit;

namespace NetTrello.Tests;

/// <summary>
/// Aggregate ordering and optimistic concurrency. Every rejected MoveCard is followed by a
/// whole-board snapshot comparison that now includes Version and UpdatedAt, so "the operation
/// failed but the board still looks touched" is a failing test rather than an unchecked detail.
/// </summary>
public sealed class BoardAggregateTests
{
    [Fact]
    public void MoveCard_ToAMissingColumn_DoesNotRemoveTheCardFromItsSource()
    {
        var fixture = new BoardFixture();
        var before = BoardFixture.Describe(fixture.Board);

        var error = Assert.Throws<DomainException>(() =>
            fixture.Board.MoveCard(fixture.First.Id, Guid.NewGuid(), 0, BoardFixture.Now));

        Assert.Equal("column.not_found", error.Code);
        Assert.Equal(before, BoardFixture.Describe(fixture.Board));
    }

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

    [Fact]
    public void MoveCard_ToANegativePosition_DoesNotMutateTheSource()
    {
        var fixture = new BoardFixture();
        var before = BoardFixture.Describe(fixture.Board);

        var error = Assert.Throws<DomainException>(() =>
            fixture.Board.MoveCard(fixture.First.Id, fixture.SourceColumnId, -1, BoardFixture.Now));

        Assert.Equal("validation.non_negative", error.Code);
        Assert.Equal(before, BoardFixture.Describe(fixture.Board));
    }

    [Fact]
    public void MoveCard_ReordersWithinItsOwnColumn_EvenWhenAnotherColumnIsFull()
    {
        var fixture = new BoardFixture();

        fixture.Board.MoveCard(fixture.Second.Id, fixture.SourceColumnId, 0, BoardFixture.Now.AddMinutes(1));

        Assert.Equal("Second,First", string.Join(',', fixture.Board.Columns[0].Cards.Select(card => card.Title)));
    }

    [Fact]
    public void CardUpdate_WithABlankLabel_PreservesEveryField()
    {
        var fixture = new BoardFixture();
        var before = BoardFixture.DescribeCard(fixture.First);

        var error = Assert.Throws<DomainException>(() => fixture.First.Update(
            "Changed", "Changed description", CardPriority.Critical, fixture.Member.Id,
            BoardFixture.Now.AddDays(3), ["ok", ""], BoardFixture.Now.AddMinutes(2)));

        Assert.Equal("validation.required", error.Code);
        Assert.Equal(before, BoardFixture.DescribeCard(fixture.First));
    }

    [Fact]
    public async Task StaleExpectedVersion_IsRejected()
    {
        var fixture = new BoardFixture();
        var version = (await fixture.ReloadAsync()).Version;

        var error = await Assert.ThrowsAsync<DomainException>(() => fixture.Service.AddColumnAsync(
            fixture.BoardId, new CreateColumnRequest("stale"), version - 1, CancellationToken.None));

        Assert.Equal("board.version_conflict", error.Code);
    }

    [Fact]
    public async Task TwoWritesWithTheSameExpectedVersion_LeaveExactlyOneWinner()
    {
        var fixture = new BoardFixture();
        var version = (await fixture.ReloadAsync()).Version;

        var results = await Task.WhenAll(
            Capture(() => fixture.Service.AddColumnAsync(fixture.BoardId, new CreateColumnRequest("one"), version, CancellationToken.None)),
            Capture(() => fixture.Service.AddColumnAsync(fixture.BoardId, new CreateColumnRequest("two"), version, CancellationToken.None)));

        Assert.Equal(1, results.Count(succeeded => succeeded));
        Assert.Equal(1, results.Count(succeeded => !succeeded));
    }

    [Fact]
    public async Task TheRepositoryHandsOutAnIsolatedClone()
    {
        var fixture = new BoardFixture();

        var clone = await fixture.ReloadAsync();
        clone.Rename("mutated clone", "should not persist", BoardFixture.Now.AddMinutes(3));

        var reread = await fixture.ReloadAsync();
        Assert.Equal("Checks", reread.Name);
    }

    private static async Task<bool> Capture(Func<Task> action)
    {
        try
        {
            await action();
            return true;
        }
        catch (DomainException)
        {
            return false;
        }
    }
}
