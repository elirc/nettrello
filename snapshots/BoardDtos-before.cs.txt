using NetTrello.Api.Domain;

namespace NetTrello.Api.Contracts;

public sealed record BoardSummaryResponse(Guid Id, string Name, string Description, int CardCount, DateTimeOffset UpdatedAt);

public sealed record BoardDetailResponse(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyList<ColumnResponse> Columns,
    IReadOnlyList<BoardMemberResponse> Members,
    int Version,
    DateTimeOffset UpdatedAt);

public sealed record ColumnResponse(Guid Id, string Title, int Position, int CardLimit, IReadOnlyList<CardResponse> Cards);

public sealed record CardResponse(
    Guid Id,
    string Title,
    string Description,
    CardPriority Priority,
    Guid? AssigneeId,
    string? AssigneeName,
    DateTimeOffset? DueDate,
    IReadOnlyList<string> Labels,
    int Position,
    DateTimeOffset UpdatedAt);

public sealed record BoardMemberResponse(Guid Id, string DisplayName, string Email);

public sealed record CreateBoardRequest(string Name, string? Description);

public sealed record CreateColumnRequest(string Title, int CardLimit = 0);

public sealed record CreateCardRequest(
    string Title,
    string? Description,
    CardPriority Priority = CardPriority.Medium,
    Guid? AssigneeId = null,
    DateTimeOffset? DueDate = null,
    IReadOnlyList<string>? Labels = null);

public sealed record UpdateCardRequest(
    string Title,
    string? Description,
    CardPriority Priority,
    Guid? AssigneeId,
    DateTimeOffset? DueDate,
    IReadOnlyList<string>? Labels);

public sealed record MoveCardRequest(Guid TargetColumnId, int Position);

public sealed record MoveColumnRequest(int Position);
