using System.Text.RegularExpressions;
using Shared;

namespace Domain;

public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex E164Regex = new(@"^\+[1-9]\d{7,14}$", RegexOptions.Compiled);

    public string Value { get; }

    public PhoneNumber(string value)
    {
        string normalizedValue = Guard.NotEmpty(value, nameof(value)).Trim();
        if (!E164Regex.IsMatch(normalizedValue))
        {
            throw new ArgumentException("Phone number must follow E.164 format (e.g., +14155552671).", nameof(value));
        }

        Value = normalizedValue;
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
