using System.Collections.Concurrent;
using NetTrello.Api.Application;
using NetTrello.Api.Domain;

namespace NetTrello.Api.Infrastructure;

public sealed class InMemoryBoardRepository : IBoardRepository
{
    private readonly ConcurrentDictionary<Guid, Board> _boards = new();
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public InMemoryBoardRepository()
    {
        var now = DateTimeOffset.UtcNow;
        var members = new[]
        {
            new BoardMember(Guid.Parse("8d092799-8d9f-4bc6-92e2-1a17decb6245"), "Avery Johnson", "avery@example.com"),
            new BoardMember(Guid.Parse("cb34862e-f13d-4444-a263-84dc3840c451"), "Mina Patel", "mina@example.com"),
            new BoardMember(Guid.Parse("40b86870-2a18-4d3b-8723-a0f948414bc0"), "Jordan Lee", "jordan@example.com")
        };

        var board = new Board(Guid.Parse("78e3d235-44e3-4668-aa9b-81583d4a5fd5"), "Product launch", "Plan the v1 rollout from discovery to release.", now, members: members);
        var backlog = board.AddColumn("Backlog", 0, now);
        var doing = board.AddColumn("In progress", 4, now);
        board.AddColumn("Review", 3, now);
        board.AddColumn("Done", 0, now);

        board.AddCard(backlog.Id, "Interview beta customers", "Capture the top three workflow pains before launch.", CardPriority.High, members[0].Id, now.AddDays(4), ["research", "customer"], now);
        board.AddCard(backlog.Id, "Write onboarding checklist", "Turn first-run setup into a repeatable list.", CardPriority.Medium, members[1].Id, now.AddDays(7), ["docs"], now);
        board.AddCard(doing.Id, "Finalize pricing page copy", "Keep it plain, specific, and legally reviewed.", CardPriority.Critical, members[2].Id, now.AddDays(2), ["marketing", "legal"], now);

        _boards[board.Id] = board;
    }

    public Task<IReadOnlyList<Board>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<Board> boards = _boards.Values.Select(board => board.Clone()).ToArray();
        return Task.FromResult(boards);
    }

    public Task<Board?> GetAsync(Guid boardId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var board = _boards.TryGetValue(boardId, out var stored) ? stored.Clone() : null;
        return Task.FromResult(board);
    }

    public async Task<Board> AddAsync(Board board, CancellationToken cancellationToken)
    {
        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            if (!_boards.TryAdd(board.Id, board.Clone()))
            {
                throw DomainException.Conflict("board.duplicate", "A board with this id already exists.");
            }

            return board.Clone();
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<Board> UpdateAsync(Guid boardId, Func<Board, Board> update, CancellationToken cancellationToken)
    {
        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            if (!_boards.TryGetValue(boardId, out var existing))
            {
                throw DomainException.NotFound("board.not_found", "Board was not found.");
            }

            var updated = update(existing.Clone());
            _boards[boardId] = updated.Clone();
            return updated.Clone();
        }
        finally
        {
            _writeLock.Release();
        }
    }
}
