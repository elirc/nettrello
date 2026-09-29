namespace NetTrello.Api.Application;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
