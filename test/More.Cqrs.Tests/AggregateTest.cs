namespace More.Domain
{
    using Example;
    using FluentAssertions;
    using More.Domain.Events;
    using System;
    using System.Linq;
    using Xunit;
    using static System.DateTimeOffset;
    using static System.Guid;

    public class AggregateTest
    {
        [Fact]
        public void replay_should_throw_exception_when_event_is_not_mapped_to_a_method()
        {
            // arrange
            var person = new Person( NewGuid(), "John", "Doe", Now );
            var history = new IEvent[] { new Hallucination() };

            // act
            Action apply = () => person.ReplayAll( history );

            // assert
            apply.Should().Throw<MissingMemberException>();
        }

        [Fact]
        public void create_snapshot_should_be_unsupported_by_default()
        {
            // arrange
            var person = new Person( NewGuid(), "John", "Doe", Now );

            // act
            Action createSnapshot = () => person.CreateSnapshot();

            // assert
            createSnapshot.Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void record_should_track_uncommitted_events()
        {
            // arrange
            var person = default( Person );
            var id = NewGuid();
            var birthday = Now;

            // act
            person = new Person( id, "John", "Doe", birthday );

            // assert
            person.IsChanged.Should().BeTrue();
            person.UncommittedEvents.Should().BeEquivalentTo(
                new[] { new { AggregateId = id, FirstName = "John", LastName = "Doe", Date = birthday } },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public void accept_changes_should_update_state()
        {
            // arrange
            var person = new Person( NewGuid(), "Jane", "Smith", Now.AddYears( -3 ) );
            var spouse = new Person( NewGuid(), "John", "Doe", Now );
            var version = 0;

            person.Marry( spouse, Now );
            person.ChangeLastName( "Doe" );

            foreach ( var @event in person.UncommittedEvents )
            {
                @event.Version = version++;
            }

            // act
            person.AcceptChanges();

            // assert
            person.IsChanged.Should().BeFalse();
            person.UncommittedEvents.Should().BeEmpty();
            person.Version.Should().Be( 2 );
        }

        [Fact]
        public void replay_all_should_load_aggregate_from_history()
        {
            // arrange
            var id = NewGuid();
            var birthday = Now.AddYears( -3 );
            var person = new Person( id, "Jane", "Smith", birthday );
            var spouse = new Person( NewGuid(), "John", "Doe", Now );
            var version = 0;

            person.Marry( spouse, Now );
            person.ChangeLastName( "Doe" );

            foreach ( var @event in person.UncommittedEvents )
            {
                @event.Version = version++;
            }

            var lifeExperiences = person.UncommittedEvents.ToArray();
            var twin = new Person();

            person.AcceptChanges();

            // act
            twin.ReplayAll( lifeExperiences );

            // assert
            twin.Equals( person ).Should().BeTrue();
            twin.Version.Should().Be( 2 );
        }
    }
}