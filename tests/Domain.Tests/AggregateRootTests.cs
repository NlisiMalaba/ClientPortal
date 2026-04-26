using Domain;

namespace Domain.Tests;

public sealed class AggregateRootTests
{
    [Fact]
    public void AddDomainEvent_AddsEventToCollection()
    {
        TestAggregate aggregate = new(Guid.NewGuid());

        aggregate.RaiseCreated();

        Assert.Single(aggregate.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        TestAggregate aggregate = new(Guid.NewGuid());
        aggregate.RaiseCreated();
        aggregate.RaiseCreated();

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void RemoveDomainEvent_RemovesSpecificEvent()
    {
        TestAggregate aggregate = new(Guid.NewGuid());
        CreatedDomainEvent first = new();
        CreatedDomainEvent second = new();
        aggregate.Raise(first);
        aggregate.Raise(second);

        aggregate.Unraise(first);

        Assert.Single(aggregate.DomainEvents);
        Assert.Contains(second, aggregate.DomainEvents);
    }

    private sealed class TestAggregate : AggregateRoot<Guid>
    {
        public TestAggregate(Guid id)
            : base(id)
        {
        }

        public void RaiseCreated()
        {
            AddDomainEvent(new CreatedDomainEvent());
        }

        public void Raise(IDomainEvent @event)
        {
            AddDomainEvent(@event);
        }

        public void Unraise(IDomainEvent @event)
        {
            RemoveDomainEvent(@event);
        }
    }

    private sealed record CreatedDomainEvent : IDomainEvent;
}
