using Shared;

namespace Domain;

public sealed class Permission : ValueObject
{
    public string Value { get; }

    public Permission(string value)
    {
        string normalizedValue = Guard.NotEmpty(value, nameof(value)).Trim().ToLowerInvariant();
        string[] segments = normalizedValue.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length < 2)
        {
            throw new ArgumentException("Permission must follow 'resource.action' format.", nameof(value));
        }

        foreach (string segment in segments)
        {
            if (segment.Any(ch => !(char.IsAsciiLetterOrDigit(ch) || ch is ':' or '_' or '-')))
            {
                throw new ArgumentException("Permission can only contain letters, digits, ':', '_' and '-'.", nameof(value));
            }
        }

        Value = string.Join('.', segments);
    }

    public override string ToString()
    {
        return Value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
