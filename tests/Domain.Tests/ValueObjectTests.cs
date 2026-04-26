using Domain;

namespace Domain.Tests;

public sealed class ValueObjectTests
{
    [Fact]
    public void Equals_ReturnsTrue_WhenEqualityComponentsMatch()
    {
        Coordinate left = new(10, 20);
        Coordinate right = new(10, 20);

        Assert.True(left.Equals(right));
        Assert.True(left == right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenEqualityComponentsDoNotMatch()
    {
        Coordinate left = new(10, 20);
        Coordinate right = new(10, 21);

        Assert.False(left.Equals(right));
        Assert.True(left != right);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenRuntimeTypesDiffer()
    {
        Coordinate valueObject = new(1, 1);
        OtherCoordinate other = new(1, 1);

        Assert.False(valueObject.Equals(other));
    }

    private sealed class Coordinate : ValueObject
    {
        private readonly int _x;
        private readonly int _y;

        public Coordinate(int x, int y)
        {
            _x = x;
            _y = y;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return _x;
            yield return _y;
        }
    }

    private sealed class OtherCoordinate : ValueObject
    {
        private readonly int _x;
        private readonly int _y;

        public OtherCoordinate(int x, int y)
        {
            _x = x;
            _y = y;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return _x;
            yield return _y;
        }
    }
}
