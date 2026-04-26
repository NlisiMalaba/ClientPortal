using Domain;

namespace Domain.Tests;

public sealed class EntityTests
{
    [Fact]
    public void Equals_ReturnsTrue_WhenEntitiesHaveSameTypeAndId()
    {
        Guid id = Guid.NewGuid();
        TestEntity left = new(id);
        TestEntity right = new(id);

        Assert.True(left.Equals(right));
        Assert.True(left == right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenEntitiesHaveDifferentRuntimeTypes()
    {
        Guid id = Guid.NewGuid();
        TestEntity left = new(id);
        OtherEntity right = new(id);

        Assert.False(left.Equals(right));
        Assert.True(left != right);
    }

    [Fact]
    public void Constructor_Throws_WhenGuidIdIsDefault()
    {
        Assert.Throws<ArgumentException>(() => new TestEntity(Guid.Empty));
    }

    [Fact]
    public void Constructor_Throws_WhenStringIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new StringIdEntity(" "));
    }

    [Fact]
    public void MarkUpdated_UpdatesUpdatedAtTimestamp()
    {
        TestEntity entity = new(Guid.NewGuid());
        DateTime previousUpdatedAt = entity.UpdatedAt;

        entity.Touch();

        Assert.True(entity.UpdatedAt > previousUpdatedAt);
    }

    private sealed class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id)
            : base(id)
        {
        }

        public void Touch()
        {
            MarkUpdated();
        }
    }

    private sealed class OtherEntity : Entity<Guid>
    {
        public OtherEntity(Guid id)
            : base(id)
        {
        }
    }

    private sealed class StringIdEntity : Entity<string>
    {
        public StringIdEntity(string id)
            : base(id)
        {
        }
    }
}
