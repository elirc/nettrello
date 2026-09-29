namespace NetTrello.Api.Domain;

public static class Guard
{
    public static string Required(string? value, string name, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw DomainException.BadRequest("validation.required", $"{name} is required.");
        }

        if (normalized.Length > maxLength)
        {
            throw DomainException.BadRequest("validation.max_length", $"{name} must be {maxLength} characters or less.");
        }

        return normalized;
    }

    public static string Optional(string? value, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length > maxLength)
        {
            throw DomainException.BadRequest("validation.max_length", $"Value must be {maxLength} characters or less.");
        }

        return normalized;
    }

    public static int NonNegative(int value, string name)
    {
        if (value < 0)
        {
            throw DomainException.BadRequest("validation.non_negative", $"{name} must be zero or greater.");
        }

        return value;
    }
}
