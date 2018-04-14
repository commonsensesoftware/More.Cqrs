namespace More.Domain.Events
{
    using FluentAssertions;
    using System;
    using System.Linq;
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
            var events = await EventStore.Load( aggregateId, CancellationToken.None );
            Action enumerate = () => events.ToArray();

            // assert
            enumerate.Should().Throw<AggregateNotFoundException>();
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

            await EventStore.Save( aggregateId, savedEvents, ExpectedVersion.Initial, CancellationToken.None );

            // act
            var loadedEvents = await EventStore.Load( aggregateId, CancellationToken.None );

            // assert
            loadedEvents.Should().BeEquivalentTo( savedEvents );
        }

        [Fact]
        public async Task save_should_throw_exception_when_concurrency_conflict_occurs()
        {
            // arrange
            var aggregateId = NewGuid();
            var events = new IEvent[] { new AccountTransaction( aggregateId, -25m ) };

            await EventStore.Save( aggregateId, new IEvent[] { new AccountTransaction( aggregateId, 100m ) }, -1, CancellationToken.None );
            await EventStore.Save( aggregateId, new IEvent[] { new AccountTransaction( aggregateId, -20m ) }, 0, CancellationToken.None );

            // act
            Func<Task> save = () => EventStore.Save( aggregateId, events, 0, CancellationToken.None );

            // assert
            save.Should().Throw<ConcurrencyException>();
        }
    }
}