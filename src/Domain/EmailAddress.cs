using System.Net.Mail;
using Shared;

namespace Domain;

public sealed class EmailAddress : ValueObject
{
    public string Value { get; }

    public EmailAddress(string value)
    {
        string normalizedValue = Guard.NotEmpty(value, nameof(value)).Trim();

        try
        {
            MailAddress parsed = new(normalizedValue);
            if (!string.Equals(parsed.Address, normalizedValue, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Email address has an invalid format.", nameof(value));
            }
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Email address has an invalid format.", nameof(value), ex);
        }

        Value = normalizedValue.ToLowerInvariant();
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
