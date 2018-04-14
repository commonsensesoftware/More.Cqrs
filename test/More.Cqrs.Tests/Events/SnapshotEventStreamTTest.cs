namespace More.Domain.Events
{
    using FluentAssertions;
    using System;
    using System.Linq;
    using Xunit;
    using static System.Guid;

    public class SnapshotEventStreamTTest
    {
        [Fact]
        public void snapshot_stream_should_only_enumerate_events_on_and_after_a_snapshot()
        {
            // arrange
            var id = NewGuid();
            var events = new IEvent[]
            {
                new AnyEvent(){ AggregateId = id, Version = 0 },
                new AnyEvent(){ AggregateId = id, Version = 1 },
                new AnyEvent(){ AggregateId = id, Version = 2 },
                new AnyEvent(){ AggregateId = id, Version = 3 },
                new AnyEvent(){ AggregateId = id, Version = 4 },
                new AnyEvent(){ AggregateId = id, Version = 5 },
                new AnyEvent(){ AggregateId = id, Version = 6 },
                new AnyEvent(){ AggregateId = id, Version = 7 },
                new AnyEvent(){ AggregateId = id, Version = 8 },
                new AnyEvent(){ AggregateId = id, Version = 9 },
            };
            var snapshot = new Snapshot() { Id = id, Version = 6 };
            var truncatedEvents = events.Skip( 7 ).ToList();
            var stream = new SnapshotEventStream<Guid>( events, snapshot );

            truncatedEvents.Insert( 0, snapshot );

            // act
            var results = stream.ToArray();

            // assert
            results.Should().BeEquivalentTo( truncatedEvents );
        }

        class AnyEvent : Event { }

        class Snapshot : SnapshotEvent<Guid> { }
    }
}