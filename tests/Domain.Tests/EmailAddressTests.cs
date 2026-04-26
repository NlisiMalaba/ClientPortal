using Domain;

namespace Domain.Tests;

public sealed class EmailAddressTests
{
    [Fact]
    public void Constructor_Throws_WhenEmailIsInvalid()
    {
        Assert.Throws<ArgumentException>(() => new EmailAddress("invalid-email"));
    }

    [Fact]
    public void Constructor_NormalizesToLowerCase()
    {
        EmailAddress emailAddress = new("  John.Doe@Example.COM ");

        Assert.Equal("john.doe@example.com", emailAddress.Value);
    }

    [Fact]
    public void Equality_IsStructural()
    {
        EmailAddress left = new("John@Example.com");
        EmailAddress right = new("john@example.com");

        Assert.True(left == right);
    }

    [Fact]
    public void Property_IsImmutable()
    {
        Assert.False(typeof(EmailAddress).GetProperty(nameof(EmailAddress.Value))!.CanWrite);
    }
}
