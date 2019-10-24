namespace More.Domain.Events
{
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Guid;

    public class EventStoreTest : DatabaseTest<EventStoreFixture<Guid>>
    {
        public EventStoreTest( EventStoreFixture<Guid> setup ) : base( setup ) =>
            EventStore = new SqlEventStore<Guid>( setup.Persistence, setup.Configuration );

        SqlEventStore<Guid> EventStore { get; }

        [Fact]
        public async Task load_should_throw_exception_when_aggregate_does_not_exist()
        {
            // arrange
            var aggregateId = NewGuid();

            // act
            var events = EventStore.Load( aggregateId );
            Func<Task> moveNextAsync = () => events.GetAsyncEnumerator().MoveNextAsync().AsTask();

            // assert
            await moveNextAsync.Should().ThrowAsync<AggregateNotFoundException>();
        }

        [Fact]
        public async Task load_should_retrieve_saved_events()
        {
            // arrange
            var aggregateId = NewGuid();
            var savedEvents = new IEvent[]
            {
                new AccountTransaction( aggregateId, 100m ),
                new AccountTransaction( aggregateId, -20m ),
                new AccountTransaction( aggregateId, -20m ),
                new AccountTransaction( aggregateId, -10m ),
                new AccountTransaction( aggregateId, 100m ),
            };

            await EventStore.Save( aggregateId, savedEvents, ExpectedVersion.Initial, default );

            // act
            var loadedEvents = new List<IEvent>();

            await foreach ( var @event in EventStore.Load( aggregateId ) )
            {
                loadedEvents.Add( @event );
            }

            // assert
            loadedEvents.Should().BeEquivalentTo( savedEvents );
        }

        [Fact]
        public async Task save_should_throw_exception_when_concurrency_conflict_occurs()
        {
            // arrange
            var aggregateId = NewGuid();
            var events = new IEvent[] { new AccountTransaction( aggregateId, -25m ) };

            await EventStore.Save( aggregateId, new IEvent[] { new AccountTransaction( aggregateId, 100m ) }, -1, default );
            await EventStore.Save( aggregateId, new IEvent[] { new AccountTransaction( aggregateId, -20m ) }, 0, default );

            // act
            Func<Task> save = () => EventStore.Save( aggregateId, events, 0, CancellationToken.None );

            // assert
            await save.Should().ThrowAsync<ConcurrencyException>();
        }
    }
}