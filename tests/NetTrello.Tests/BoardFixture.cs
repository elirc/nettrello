using NetTrello.Api.Application;
using NetTrello.Api.Contracts;
using NetTrello.Api.Domain;
using NetTrello.Api.Infrastructure;
using System.Text.Json;

namespace NetTrello.Tests;

/// <summary>
/// The shared arrangement every domain/service test starts from: a board with a "Source" column
/// (unlimited) holding two cards and a "Target" column limited to two cards that is already full.
/// This is the same fixture the old console harness built, kept in one place so each [Fact] can
/// start from a clean copy instead of inheriting whatever the previous check left behind.
/// </summary>
public sealed class BoardFixture
{
    public static readonly DateTimeOffset Now = new(2026, 9, 12, 12, 0, 0, TimeSpan.Zero);

    public BoardFixture()
    {
        Repository = new InMemoryBoardRepository();
        Service = new BoardService(Repository, new FixedClock(Now));

        var seed = new Board(Guid.NewGuid(), "Checks", "Regression board", Now);
        var source = seed.AddColumn("Source", 0, Now);
        var target = seed.AddColumn("Target", 2, Now);
        Member = new BoardMember(Guid.NewGuid(), "Test Member", "test@example.com");

        Board = new Board(seed.Id, seed.Name, seed.Description, Now,
            columns: new[] { source, target }, members: new[] { Member });

        SourceColumnId = source.Id;
        TargetColumnId = target.Id;

        First = Board.AddCard(source.Id, "First", "Keep me", CardPriority.High, Member.Id, Now.AddDays(1), ["one", "two"], Now);
        Second = Board.AddCard(source.Id, "Second", "Keep too", CardPriority.Medium, null, null, ["two"], Now);
        Board.AddCard(target.Id, "Target one", null, CardPriority.Low, null, null, [], Now);
        Board.AddCard(target.Id, "Target two", null, CardPriority.Low, null, null, [], Now);

        Repository.AddAsync(Board, CancellationToken.None).GetAwaiter().GetResult();
    }

    public InMemoryBoardRepository Repository { get; }
    public BoardService Service { get; }
    public Board Board { get; }
    public BoardMember Member { get; }
    public Guid SourceColumnId { get; }
    public Guid TargetColumnId { get; }
    public Card First { get; }
    public Card Second { get; }
    public Guid BoardId => Board.Id;

    public async Task<Board> ReloadAsync() =>
        await Repository.GetAsync(Board.Id, CancellationToken.None)
        ?? throw new InvalidOperationException("board missing");

    /// <summary>
    /// A whole-board snapshot used to assert "nothing changed" after a rejected operation.
    /// It now includes Version and UpdatedAt as well as the columns and cards, so a rejected
    /// MoveCard that bumped the board's version would be caught.
    /// </summary>
    public static string Describe(Board? board) => board is null
        ? "<null>"
        : JsonSerializer.Serialize(new
        {
            board.Version,
            board.UpdatedAt,
            Columns = board.Columns.Select(column => new
            {
                column.Id,
                column.Title,
                column.CardLimit,
                Cards = column.Cards.Select(card => new
                {
                    card.Id,
                    card.Title,
                    card.Description,
                    card.Priority,
                    card.AssigneeId,
                    card.DueDate,
                    card.Labels,
                    card.Position,
                    card.UpdatedAt
                })
            })
        });

    public static string DescribeCard(Card card) => JsonSerializer.Serialize(new
    {
        card.Id,
        card.Title,
        card.Description,
        card.Priority,
        card.AssigneeId,
        card.DueDate,
        card.Labels,
        card.Position,
        card.UpdatedAt
    });

    public static UpdateCardRequest Request(string json) =>
        JsonSerializer.Deserialize<UpdateCardRequest>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))
        ?? throw new InvalidOperationException("request did not deserialize");

    public static JsonElement Element(string value) =>
        JsonDocument.Parse(JsonSerializer.Serialize(value)).RootElement.Clone();

    public static JsonElement ElementArray(params string[] values) =>
        JsonDocument.Parse(JsonSerializer.Serialize(values)).RootElement.Clone();

    private sealed class FixedClock(DateTimeOffset value) : IClock
    {
        public DateTimeOffset UtcNow => value;
    }
}
