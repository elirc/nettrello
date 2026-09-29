namespace NetTrello.Api.Domain;

public sealed class BoardMember
{
    public BoardMember(Guid id, string displayName, string email)
    {
        Id = id;
        DisplayName = Guard.Required(displayName, nameof(displayName), 80);
        Email = Guard.Required(email, nameof(email), 160);
    }

    public Guid Id { get; }

    public string DisplayName { get; }

    public string Email { get; }
}
