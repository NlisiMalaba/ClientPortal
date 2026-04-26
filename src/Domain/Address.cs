using Shared;

namespace Domain;

public sealed class Address : ValueObject
{
    public string Line1 { get; }

    public string? Line2 { get; }

    public string City { get; }

    public string StateOrProvince { get; }

    public string PostalCode { get; }

    public string Country { get; }

    public Address(
        string line1,
        string? line2,
        string city,
        string stateOrProvince,
        string postalCode,
        string country)
    {
        Line1 = Guard.NotEmpty(line1, nameof(line1)).Trim();
        Line2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim();
        City = Guard.NotEmpty(city, nameof(city)).Trim();
        StateOrProvince = Guard.NotEmpty(stateOrProvince, nameof(stateOrProvince)).Trim();
        PostalCode = Guard.NotEmpty(postalCode, nameof(postalCode)).Trim();
        Country = Guard.NotEmpty(country, nameof(country)).Trim().ToUpperInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Line1;
        yield return Line2;
        yield return City;
        yield return StateOrProvince;
        yield return PostalCode;
        yield return Country;
    }
}
