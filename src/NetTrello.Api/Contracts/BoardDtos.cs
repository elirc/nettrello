using NetTrello.Api.Domain;
using System.Text.Json;
using System.Text.Json.Serialization;

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

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class UpdateCardRequest
{
    // Nonnullable JsonElement preserves Undefined (omitted) from an explicit
    // JSON null. This is essential for PATCH semantics.
    public JsonElement Title { get; init; }

    public JsonElement Description { get; init; }

    public JsonElement Priority { get; init; }

    public JsonElement AssigneeId { get; init; }

    public JsonElement DueDate { get; init; }

    public JsonElement Labels { get; init; }

    public CardUpdateValues ResolveAgainst(Card current)
    {
        if (Title.ValueKind == JsonValueKind.Undefined && Description.ValueKind == JsonValueKind.Undefined && Priority.ValueKind == JsonValueKind.Undefined && AssigneeId.ValueKind == JsonValueKind.Undefined && DueDate.ValueKind == JsonValueKind.Undefined && Labels.ValueKind == JsonValueKind.Undefined)
        {
            throw DomainException.BadRequest("card.patch_empty", "At least one card field is required.");
        }

        return new CardUpdateValues(
            ReadRequiredString(Title, current.Title, "title"),
            ReadNullableString(Description, current.Description, "description"),
            ReadPriority(Priority, current.Priority),
            ReadNullableGuid(AssigneeId, current.AssigneeId, "assigneeId"),
            ReadNullableDate(DueDate, current.DueDate, "dueDate"),
            ReadNullableLabels(Labels, current.Labels));
    }

    private static string ReadRequiredString(JsonElement value, string current, string name)
    {
        if (value.ValueKind == JsonValueKind.Undefined)
        {
            return current;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            throw DomainException.BadRequest("card.field_invalid", $"{name} must be a string.");
        }

        return value.GetString() ?? throw DomainException.BadRequest("card.field_invalid", $"{name} cannot be null.");
    }

    private static string? ReadNullableString(JsonElement value, string current, string name)
    {
        if (value.ValueKind == JsonValueKind.Undefined) return current;
        if (value.ValueKind == JsonValueKind.Null) return null;
        if (value.ValueKind != JsonValueKind.String)
        {
            throw DomainException.BadRequest("card.field_invalid", $"{name} must be a string or null.");
        }

        return value.GetString();
    }

    private static CardPriority ReadPriority(JsonElement value, CardPriority current)
    {
        if (value.ValueKind == JsonValueKind.Undefined) return current;
        var element = value;
        if (element.ValueKind == JsonValueKind.String && Enum.TryParse<CardPriority>(element.GetString(), true, out var parsed) && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var number) && Enum.IsDefined((CardPriority)number))
        {
            return (CardPriority)number;
        }

        throw DomainException.BadRequest("card.priority_invalid", "priority must be a known card priority.");
    }

    private static Guid? ReadNullableGuid(JsonElement value, Guid? current, string name)
    {
        if (value.ValueKind == JsonValueKind.Undefined) return current;
        if (value.ValueKind == JsonValueKind.Null) return null;
        if (value.ValueKind == JsonValueKind.String && Guid.TryParse(value.GetString(), out var parsed)) return parsed;
        throw DomainException.BadRequest("card.field_invalid", $"{name} must be a GUID or null.");
    }

    private static DateTimeOffset? ReadNullableDate(JsonElement value, DateTimeOffset? current, string name)
    {
        if (value.ValueKind == JsonValueKind.Undefined) return current;
        if (value.ValueKind == JsonValueKind.Null) return null;
        if (value.ValueKind == JsonValueKind.String && value.TryGetDateTimeOffset(out var parsed)) return parsed;
        throw DomainException.BadRequest("card.field_invalid", $"{name} must be an ISO date or null.");
    }

    private static IReadOnlyList<string>? ReadNullableLabels(JsonElement value, IReadOnlyList<string> current)
    {
        if (value.ValueKind == JsonValueKind.Undefined) return current;
        if (value.ValueKind == JsonValueKind.Null) return null;
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw DomainException.BadRequest("card.labels_invalid", "labels must be an array of strings or null.");
        }

        var labels = new List<string>();
        foreach (var item in value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                throw DomainException.BadRequest("card.labels_invalid", "labels must contain only strings.");
            }

            labels.Add(item.GetString() ?? string.Empty);
        }

        return labels;
    }
}

public sealed record CardUpdateValues(
    string Title,
    string? Description,
    CardPriority Priority,
    Guid? AssigneeId,
    DateTimeOffset? DueDate,
    IReadOnlyList<string>? Labels);

public sealed record MoveCardRequest(Guid TargetColumnId, int Position);

public sealed record MoveColumnRequest(int Position);
