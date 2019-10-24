namespace More.Domain
{
    using Example;
    using FluentAssertions;
    using More.Domain.Events;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using static System.DateTimeOffset;
    using static System.Guid;
    using static System.Threading.Tasks.Task;

    public class RepositoryTest
    {
        [Fact]
        public async Task save_should_update_state()
        {
            // arrange
            var repository = new Repository<Person>( new SimpleEventStore() );
            var person = new Person( NewGuid(), "Jane", "Smith", Now.AddYears( -3 ) );
            var spouse = new Person( NewGuid(), "John", "Doe", Now );

            person.Marry( spouse, Now );
            person.ChangeLastName( "Doe" );

            // act
            await repository.Save( person, ExpectedVersion.Initial, CancellationToken.None );

            // assert
            person.IsChanged.Should().BeFalse();
            person.UncommittedEvents.Should().BeEmpty();
            person.Version.Should().Be( 2 );
        }

        [Fact]
        public async Task single_should_load_aggregate()
        {
            // arrange
            var id = NewGuid();
            var birthday = Now.AddYears( -3 );
            var anniversary = Now;
            var person = new Person( id, "Jane", "Smith", birthday );
            var spouse = new Person( NewGuid(), "John", "Doe", Now );

            person.Marry( spouse, anniversary );
            person.ChangeLastName( "Doe" );

            var events = new IEvent[]
            {
                new Born( id, "Jane", "Smith", birthday ),
                new Married( id, spouse.Id, anniversary ),
                new LastNameChanged( id, "Doe" )
            };
            var eventStore = new SimpleEventStore( new Dictionary<Guid, IEnumerable<IEvent>>() { [id] = events } );
            var repository = new Repository<Person>( eventStore );

            // act
            var twin = await repository.Single( id, CancellationToken.None );

            // assert
            twin.Equals( person ).Should().BeTrue();
        }

        class SimpleEventStore : IEventStore<Guid>
        {
            readonly IReadOnlyDictionary<Guid, IEnumerable<IEvent>> events;

            public SimpleEventStore() => events = new Dictionary<Guid, IEnumerable<IEvent>>();

            public SimpleEventStore( IReadOnlyDictionary<Guid, IEnumerable<IEvent>> events ) => this.events = events;

            public async IAsyncEnumerable<IEvent> Load( Guid aggregateId, IEventPredicate<Guid> predicate )
            {
                await Yield();

                foreach ( var @event in events[aggregateId] )
                {
                    yield return @event;
                }
            }

            public Task Save( Guid aggregateId, IEnumerable<IEvent> events, int expectedVersion, CancellationToken cancellationToken )
            {
                var version = 0;

                foreach ( var @event in events )
                {
                    @event.Version = version++;
                }

                return CompletedTask;
            }
        }
    }
}