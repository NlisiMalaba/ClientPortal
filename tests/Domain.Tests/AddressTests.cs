using Domain;

namespace Domain.Tests;

public sealed class AddressTests
{
    [Fact]
    public void Constructor_Throws_WhenRequiredFieldsAreMissing()
    {
        Assert.Throws<ArgumentException>(() => new Address("", null, "City", "State", "12345", "ZA"));
        Assert.Throws<ArgumentException>(() => new Address("Line 1", null, "", "State", "12345", "ZA"));
        Assert.Throws<ArgumentException>(() => new Address("Line 1", null, "City", "", "12345", "ZA"));
        Assert.Throws<ArgumentException>(() => new Address("Line 1", null, "City", "State", "", "ZA"));
        Assert.Throws<ArgumentException>(() => new Address("Line 1", null, "City", "State", "12345", ""));
    }

    [Fact]
    public void Constructor_NormalizesValues()
    {
        Address address = new(" 12 Main St ", " ", " Cape Town ", " Western Cape ", " 8000 ", " za ");

        Assert.Equal("12 Main St", address.Line1);
        Assert.Null(address.Line2);
        Assert.Equal("Cape Town", address.City);
        Assert.Equal("Western Cape", address.StateOrProvince);
        Assert.Equal("8000", address.PostalCode);
        Assert.Equal("ZA", address.Country);
    }

    [Fact]
    public void Equality_IsStructural()
    {
        Address left = new("Line 1", "Line 2", "City", "State", "12345", "za");
        Address right = new("Line 1", "Line 2", "City", "State", "12345", "ZA");

        Assert.True(left == right);
    }

    [Fact]
    public void Properties_AreImmutable()
    {
        Assert.False(typeof(Address).GetProperty(nameof(Address.Line1))!.CanWrite);
        Assert.False(typeof(Address).GetProperty(nameof(Address.Line2))!.CanWrite);
        Assert.False(typeof(Address).GetProperty(nameof(Address.City))!.CanWrite);
        Assert.False(typeof(Address).GetProperty(nameof(Address.StateOrProvince))!.CanWrite);
        Assert.False(typeof(Address).GetProperty(nameof(Address.PostalCode))!.CanWrite);
        Assert.False(typeof(Address).GetProperty(nameof(Address.Country))!.CanWrite);
    }
}
