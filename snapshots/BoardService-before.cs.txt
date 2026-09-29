using NetTrello.Api.Contracts;
using NetTrello.Api.Domain;

namespace NetTrello.Api.Application;

public sealed class BoardService
{
    private readonly IBoardRepository _boards;
    private readonly IClock _clock;

    public BoardService(IBoardRepository boards, IClock clock)
    {
        _boards = boards;
        _clock = clock;
    }

    public async Task<IReadOnlyList<BoardSummaryResponse>> ListBoardsAsync(CancellationToken cancellationToken)
    {
        var boards = await _boards.ListAsync(cancellationToken);
        return boards
            .OrderByDescending(board => board.UpdatedAt)
            .Select(BoardMapper.ToSummaryResponse)
            .ToArray();
    }

    public async Task<BoardDetailResponse> GetBoardAsync(Guid boardId, CancellationToken cancellationToken)
    {
        var board = await RequireBoardAsync(boardId, cancellationToken);
        return BoardMapper.ToDetailResponse(board);
    }

    public async Task<BoardDetailResponse> CreateBoardAsync(CreateBoardRequest request, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var board = new Board(Guid.NewGuid(), request.Name, request.Description, now);

        board.AddColumn("Backlog", 0, now);
        board.AddColumn("In progress", 4, now);
        board.AddColumn("Review", 3, now);
        board.AddColumn("Done", 0, now);

        var saved = await _boards.AddAsync(board, cancellationToken);
        return BoardMapper.ToDetailResponse(saved);
    }

    public Task<BoardDetailResponse> AddColumnAsync(Guid boardId, CreateColumnRequest request, int? expectedVersion, CancellationToken cancellationToken)
    {
        return MutateAsync(boardId, expectedVersion, board =>
        {
            board.AddColumn(request.Title, request.CardLimit, _clock.UtcNow);
            return board;
        }, cancellationToken);
    }

    public Task<BoardDetailResponse> MoveColumnAsync(Guid boardId, Guid columnId, MoveColumnRequest request, int? expectedVersion, CancellationToken cancellationToken)
    {
        return MutateAsync(boardId, expectedVersion, board =>
        {
            board.MoveColumn(columnId, request.Position, _clock.UtcNow);
            return board;
        }, cancellationToken);
    }

    public Task<BoardDetailResponse> AddCardAsync(Guid boardId, Guid columnId, CreateCardRequest request, int? expectedVersion, CancellationToken cancellationToken)
    {
        return MutateAsync(boardId, expectedVersion, board =>
        {
            board.AddCard(columnId, request.Title, request.Description, request.Priority, request.AssigneeId, request.DueDate, request.Labels, _clock.UtcNow);
            return board;
        }, cancellationToken);
    }

    public Task<BoardDetailResponse> UpdateCardAsync(Guid boardId, Guid cardId, UpdateCardRequest request, int? expectedVersion, CancellationToken cancellationToken)
    {
        return MutateAsync(boardId, expectedVersion, board =>
        {
            board.UpdateCard(cardId, request.Title, request.Description, request.Priority, request.AssigneeId, request.DueDate, request.Labels, _clock.UtcNow);
            return board;
        }, cancellationToken);
    }

    public Task<BoardDetailResponse> MoveCardAsync(Guid boardId, Guid cardId, MoveCardRequest request, int? expectedVersion, CancellationToken cancellationToken)
    {
        return MutateAsync(boardId, expectedVersion, board =>
        {
            board.MoveCard(cardId, request.TargetColumnId, request.Position, _clock.UtcNow);
            return board;
        }, cancellationToken);
    }

    public Task<BoardDetailResponse> ArchiveCardAsync(Guid boardId, Guid cardId, int? expectedVersion, CancellationToken cancellationToken)
    {
        return MutateAsync(boardId, expectedVersion, board =>
        {
            board.ArchiveCard(cardId, _clock.UtcNow);
            return board;
        }, cancellationToken);
    }

    private async Task<Board> RequireBoardAsync(Guid boardId, CancellationToken cancellationToken)
    {
        return await _boards.GetAsync(boardId, cancellationToken)
            ?? throw DomainException.NotFound("board.not_found", "Board was not found.");
    }

    private async Task<BoardDetailResponse> MutateAsync(Guid boardId, int? expectedVersion, Func<Board, Board> update, CancellationToken cancellationToken)
    {
        var updated = await _boards.UpdateAsync(boardId, board =>
        {
            if (expectedVersion is not null && board.Version != expectedVersion.Value)
            {
                throw DomainException.Conflict("board.version_conflict", "The board changed since it was last loaded. Refresh and try again.");
            }

            return update(board);
        }, cancellationToken);

        return BoardMapper.ToDetailResponse(updated);
    }
}
