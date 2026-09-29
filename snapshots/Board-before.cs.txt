namespace NetTrello.Api.Domain;

public sealed class Board
{
    private readonly List<BoardColumn> _columns;
    private readonly List<BoardMember> _members;

    public Board(Guid id, string name, string? description, DateTimeOffset now, IReadOnlyCollection<BoardColumn>? columns = null, IReadOnlyCollection<BoardMember>? members = null)
    {
        Id = id;
        Name = Guard.Required(name, nameof(name), 100);
        Description = Guard.Optional(description, 500);
        CreatedAt = now;
        UpdatedAt = now;
        Version = 1;
        _columns = columns?.OrderBy(column => column.Position).Select(column => column.Clone()).ToList() ?? [];
        _members = members?.ToList() ?? [];
        ReindexColumns();
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public int Version { get; private set; }

    public IReadOnlyList<BoardColumn> Columns => _columns;

    public IReadOnlyList<BoardMember> Members => _members;

    public void Rename(string name, string? description, DateTimeOffset now)
    {
        Name = Guard.Required(name, nameof(name), 100);
        Description = Guard.Optional(description, 500);
        Touch(now);
    }

    public BoardColumn AddColumn(string title, int cardLimit, DateTimeOffset now)
    {
        if (cardLimit < 0)
        {
            throw DomainException.BadRequest("validation.non_negative", "cardLimit must be zero or greater.");
        }

        var column = new BoardColumn(Guid.NewGuid(), title, _columns.Count, cardLimit);
        _columns.Add(column);
        Touch(now);
        return column;
    }

    public void MoveColumn(Guid columnId, int position, DateTimeOffset now)
    {
        var column = FindColumn(columnId);
        _columns.Remove(column);
        _columns.Insert(Math.Clamp(position, 0, _columns.Count), column);
        ReindexColumns();
        Touch(now);
    }

    public Card AddCard(Guid columnId, string title, string? description, CardPriority priority, Guid? assigneeId, DateTimeOffset? dueDate, IReadOnlyCollection<string>? labels, DateTimeOffset now)
    {
        EnsureAssigneeExists(assigneeId);
        var column = FindColumn(columnId);
        var card = new Card(Guid.NewGuid(), title, description, priority, assigneeId, dueDate, labels, column.Cards.Count, now);
        column.AddCard(card);
        Touch(now);
        return card;
    }

    public void UpdateCard(Guid cardId, string title, string? description, CardPriority priority, Guid? assigneeId, DateTimeOffset? dueDate, IReadOnlyCollection<string>? labels, DateTimeOffset now)
    {
        EnsureAssigneeExists(assigneeId);
        var card = FindCard(cardId);
        card.Update(title, description, priority, assigneeId, dueDate, labels, now);
        Touch(now);
    }

    public void MoveCard(Guid cardId, Guid targetColumnId, int position, DateTimeOffset now)
    {
        var sourceColumn = FindColumnContainingCard(cardId);
        var card = sourceColumn.RemoveCard(cardId);
        card.MoveTo(position, now);

        var targetColumn = FindColumn(targetColumnId);
        targetColumn.AddCard(card);
        Touch(now);
    }

    public void ArchiveCard(Guid cardId, DateTimeOffset now)
    {
        var sourceColumn = FindColumnContainingCard(cardId);
        sourceColumn.RemoveCard(cardId);
        Touch(now);
    }

    public Board Clone()
    {
        var clone = new Board(Id, Name, Description, CreatedAt, _columns, _members)
        {
            UpdatedAt = UpdatedAt,
            Version = Version
        };

        return clone;
    }

    private void EnsureAssigneeExists(Guid? assigneeId)
    {
        if (assigneeId is null)
        {
            return;
        }

        if (_members.All(member => member.Id != assigneeId.Value))
        {
            throw DomainException.BadRequest("member.not_found", "Assignee is not a member of this board.");
        }
    }

    private BoardColumn FindColumn(Guid columnId)
    {
        return _columns.SingleOrDefault(column => column.Id == columnId)
            ?? throw DomainException.NotFound("column.not_found", "Column was not found.");
    }

    private BoardColumn FindColumnContainingCard(Guid cardId)
    {
        return _columns.SingleOrDefault(column => column.Cards.Any(card => card.Id == cardId))
            ?? throw DomainException.NotFound("card.not_found", "Card was not found.");
    }

    private Card FindCard(Guid cardId)
    {
        return _columns.SelectMany(column => column.Cards).SingleOrDefault(card => card.Id == cardId)
            ?? throw DomainException.NotFound("card.not_found", "Card was not found.");
    }

    private void ReindexColumns()
    {
        for (var index = 0; index < _columns.Count; index++)
        {
            _columns[index].MoveTo(index);
        }
    }

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
        Version++;
    }
}
