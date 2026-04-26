using Domain;

namespace Domain.Tests;

public sealed class PhoneNumberTests
{
    [Fact]
    public void Constructor_Throws_WhenPhoneNumberIsNotE164()
    {
        Assert.Throws<ArgumentException>(() => new PhoneNumber("555-1234"));
    }

    [Fact]
    public void Constructor_AcceptsValidE164PhoneNumber()
    {
        PhoneNumber phoneNumber = new("+14155552671");

        Assert.Equal("+14155552671", phoneNumber.Value);
    }

    [Fact]
    public void Equality_IsStructural()
    {
        PhoneNumber left = new("+14155552671");
        PhoneNumber right = new("+14155552671");

        Assert.True(left == right);
    }

    [Fact]
    public void Property_IsImmutable()
    {
        Assert.False(typeof(PhoneNumber).GetProperty(nameof(PhoneNumber.Value))!.CanWrite);
    }
}
