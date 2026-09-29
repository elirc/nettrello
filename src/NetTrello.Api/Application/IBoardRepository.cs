using NetTrello.Api.Domain;

namespace NetTrello.Api.Application;

public interface IBoardRepository
{
    Task<IReadOnlyList<Board>> ListAsync(CancellationToken cancellationToken);

    Task<Board?> GetAsync(Guid boardId, CancellationToken cancellationToken);

    Task<Board> AddAsync(Board board, CancellationToken cancellationToken);

    Task<Board> UpdateAsync(Guid boardId, Func<Board, Board> update, CancellationToken cancellationToken);
}
