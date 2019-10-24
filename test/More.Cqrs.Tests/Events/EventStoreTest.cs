﻿namespace More.Domain.Events
{
    using Example;
    using FluentAssertions;
    using More.Domain.Persistence;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using static System.DateTimeOffset;
    using static System.Guid;
    using static System.Linq.Enumerable;
    using static System.Threading.Tasks.Task;

    public class EventStoreTest
    {
        [Fact]
        public async Task load_should_return_stream_that_throws_not_found_exception()
        {
            // arrange
            var id = NewGuid();
            var persistence = new SimplePersistence();
            var eventStore = new SimpleEventStore( persistence );

            // act
            await using var enumerator = eventStore.Load( id ).GetAsyncEnumerator();
            Func<Task> moveNextAsync = () => enumerator.MoveNextAsync().AsTask();

            // assert
            await moveNextAsync.Should().ThrowAsync<AggregateNotFoundException>();
        }

        [Fact]
        public async Task save_should_update_event_versions()
        {
            // arrange
            var id = NewGuid();
            var birthday = Now.AddYears( -21 );
            var events = new IEvent[]
            {
                new Born( id, "John", "Doe", birthday ),
                new Married( id, NewGuid(), Now )
            };
            var persistence = new SimplePersistence();
            var eventStore = new SimpleEventStore( persistence );
            var version = 0;
            var sequence = 0;

            // act
            await eventStore.Save( id, events, ExpectedVersion.Initial, default );

            // assert
            foreach ( var @event in events )
            {
                @event.Should().BeEquivalentTo( new { Version = version, Sequence = sequence++ }, options => options.ExcludingMissingMembers() );
            }
        }

        [Fact]
        public async Task save_should_throw_exception_when_concurrency_is_violated()
        {
            // arrange
            var id = NewGuid();
            var birthday = Now.AddYears( -21 );
            var newEvents = new IEvent[] { new Married( id, NewGuid(), Now ) };
            var persistence = new SimplePersistence();
            var eventStore = new SimpleEventStore( persistence );

            await persistence.Persist( new Commit() { Id = id, Version = 0, Events = { new Born( id, "John", "Doe", birthday ) } }, default );
            await persistence.Persist( new Commit() { Id = id, Version = 1, Events = { new Married( id, NewGuid(), Now ) } }, default );

            // act
            Func<Task> save = () => eventStore.Save( id, newEvents, 0, default );

            // assert
            await save.Should().ThrowAsync<ConcurrencyException>();
        }

        class SimpleEventStore : EventStore
        {
            public SimpleEventStore() : base( new SimplePersistence() ) { }

            public SimpleEventStore( IPersistence persistence ) : base( persistence ) { }

            protected override async IAsyncEnumerable<IEvent> OnLoad( Guid aggregateId, IEventPredicate<Guid> predicate )
            {
                var commits = ( (SimplePersistence) Persistence ).Commits;
                var events = from commit in commits
                             where commit.Id.Equals( aggregateId )
                             from @event in commit.Events
                             select @event;

                await Yield();

                foreach ( var @event in events.OrderBy( e => e.Version ).ThenBy( e => e.Sequence ) )
                {
                    yield return @event;
                }
            }
        }

        class SimplePersistence : IPersistence
        {
            readonly CommitCollection commits = new CommitCollection();

            public IReadOnlyList<Commit> Commits => commits;

            public Task Persist( Commit commit, CancellationToken cancellationToken )
            {
                try
                {
                    commits.Add( commit );
                }
                catch ( ArgumentException ex ) when ( ex.Message.StartsWith( "An item with the same key has already been added." ) )
                {
                    throw new ConcurrencyException( $"The aggregate with the identifier '{commit.Id}' is greater than or equal to version {commit.Version}." );
                }

                var sequence = 0;

                foreach ( var @event in commit.Events )
                {
                    @event.Version = commit.Version;
                    @event.Sequence = sequence++;
                }

                return CompletedTask;
            }
        }

        struct Key : IEquatable<Key>
        {
            readonly object id;
            readonly int version;

            internal Key( object id, int version )
            {
                this.id = id;
                this.version = version;
            }

            public override int GetHashCode() => HashCode.Combine( id, version );

            public override bool Equals( object obj ) => obj is Key other && Equals( other );

            public bool Equals( Key other ) => id.Equals( other.id ) && version == other.version;
        }

        class CommitCollection : KeyedCollection<Key, Commit>
        {
            internal CommitCollection() : base( EqualityComparer<Key>.Default, 0 ) { }

            protected override Key GetKeyForItem( Commit item ) => new Key( item.Id, item.Version );
        }
    }
}