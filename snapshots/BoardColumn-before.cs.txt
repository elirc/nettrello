namespace NetTrello.Api.Domain;

public sealed class BoardColumn
{
    private readonly List<Card> _cards;

    public BoardColumn(Guid id, string title, int position, int cardLimit, IReadOnlyCollection<Card>? cards = null)
    {
        Id = id;
        Title = Guard.Required(title, nameof(title), 80);
        Position = Guard.NonNegative(position, nameof(position));
        if (cardLimit < 0)
        {
            throw DomainException.BadRequest("validation.non_negative", "cardLimit must be zero or greater.");
        }

        CardLimit = cardLimit;
        _cards = cards?.OrderBy(card => card.Position).Select(card => card.Clone()).ToList() ?? [];
        EnsureCardLimit();
        ReindexCards();
    }

    public Guid Id { get; }

    public string Title { get; private set; }

    public int Position { get; private set; }

    public int CardLimit { get; private set; }

    public IReadOnlyList<Card> Cards => _cards;

    public void Rename(string title)
    {
        Title = Guard.Required(title, nameof(title), 80);
    }

    public void MoveTo(int position)
    {
        Position = Guard.NonNegative(position, nameof(position));
    }

    public void AddCard(Card card)
    {
        _cards.Insert(Math.Clamp(card.Position, 0, _cards.Count), card);
        EnsureCardLimit();
        ReindexCards();
    }

    public Card RemoveCard(Guid cardId)
    {
        var card = _cards.SingleOrDefault(item => item.Id == cardId);
        if (card is null)
        {
            throw DomainException.NotFound("card.not_found", "Card was not found.");
        }

        _cards.Remove(card);
        ReindexCards();
        return card;
    }

    public Card GetCard(Guid cardId)
    {
        return _cards.SingleOrDefault(card => card.Id == cardId)
            ?? throw DomainException.NotFound("card.not_found", "Card was not found.");
    }

    public BoardColumn Clone()
    {
        return new BoardColumn(Id, Title, Position, CardLimit, _cards);
    }

    private void ReindexCards()
    {
        for (var index = 0; index < _cards.Count; index++)
        {
            _cards[index].MoveTo(index, _cards[index].UpdatedAt);
        }
    }

    private void EnsureCardLimit()
    {
        if (CardLimit > 0 && _cards.Count > CardLimit)
        {
            throw DomainException.Conflict("column.card_limit", $"Column '{Title}' allows at most {CardLimit} cards.");
        }
    }
}
