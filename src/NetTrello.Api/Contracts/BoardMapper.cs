using NetTrello.Api.Domain;

namespace NetTrello.Api.Contracts;

public static class BoardMapper
{
    public static BoardSummaryResponse ToSummaryResponse(Board board)
    {
        return new BoardSummaryResponse(
            board.Id,
            board.Name,
            board.Description,
            board.Columns.Sum(column => column.Cards.Count),
            board.UpdatedAt);
    }

    public static BoardDetailResponse ToDetailResponse(Board board)
    {
        return new BoardDetailResponse(
            board.Id,
            board.Name,
            board.Description,
            board.Columns.Select(column => ToColumnResponse(board, column)).ToArray(),
            board.Members.Select(member => new BoardMemberResponse(member.Id, member.DisplayName, member.Email)).ToArray(),
            board.Version,
            board.UpdatedAt);
    }

    private static ColumnResponse ToColumnResponse(Board board, BoardColumn column)
    {
        return new ColumnResponse(
            column.Id,
            column.Title,
            column.Position,
            column.CardLimit,
            column.Cards.Select(card => ToCardResponse(board, card)).ToArray());
    }

    private static CardResponse ToCardResponse(Board board, Card card)
    {
        var assignee = board.Members.SingleOrDefault(member => member.Id == card.AssigneeId);
        return new CardResponse(
            card.Id,
            card.Title,
            card.Description,
            card.Priority,
            card.AssigneeId,
            assignee?.DisplayName,
            card.DueDate,
            card.Labels,
            card.Position,
            card.UpdatedAt);
    }
}
