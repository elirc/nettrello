namespace NetTrello.Api.Domain;

public sealed class Card
{
    public Card(
        Guid id,
        string title,
        string? description,
        CardPriority priority,
        Guid? assigneeId,
        DateTimeOffset? dueDate,
        IReadOnlyCollection<string>? labels,
        int position,
        DateTimeOffset now)
    {
        Id = id;
        Title = Guard.Required(title, nameof(title), 120);
        Description = Guard.Optional(description, 2_000);
        Priority = priority;
        AssigneeId = assigneeId;
        DueDate = dueDate;
        Labels = NormalizeLabels(labels);
        Position = Guard.NonNegative(position, nameof(position));
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid Id { get; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public CardPriority Priority { get; private set; }

    public Guid? AssigneeId { get; private set; }

    public DateTimeOffset? DueDate { get; private set; }

    public IReadOnlyList<string> Labels { get; private set; }

    public int Position { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(string title, string? description, CardPriority priority, Guid? assigneeId, DateTimeOffset? dueDate, IReadOnlyCollection<string>? labels, DateTimeOffset now)
    {
        Title = Guard.Required(title, nameof(title), 120);
        Description = Guard.Optional(description, 2_000);
        Priority = priority;
        AssigneeId = assigneeId;
        DueDate = dueDate;
        Labels = NormalizeLabels(labels);
        UpdatedAt = now;
    }

    public void MoveTo(int position, DateTimeOffset now)
    {
        Position = Guard.NonNegative(position, nameof(position));
        UpdatedAt = now;
    }

    public Card Clone()
    {
        var clone = new Card(Id, Title, Description, Priority, AssigneeId, DueDate, Labels, Position, CreatedAt)
        {
            UpdatedAt = UpdatedAt
        };

        return clone;
    }

    private static IReadOnlyList<string> NormalizeLabels(IReadOnlyCollection<string>? labels)
    {
        return labels?
            .Select(label => Guard.Required(label, "label", 32))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];
    }
}
