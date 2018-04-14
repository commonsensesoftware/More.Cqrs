namespace More.Domain.Messaging
{
    using FluentAssertions;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using static System.DateTimeOffset;
    using static System.Guid;

    public class MessageQueueTest : DatabaseTest<QueueFixture>
    {
        public MessageQueueTest( QueueFixture setup ) : base( setup )
        {
            MessageSender = new SqlMessageSender( setup.Configuration );
            MessageReceiver = new SqlMessageReceiver( setup.Configuration );
        }

        SqlMessageSender MessageSender { get; }

        SqlMessageReceiver MessageReceiver { get; }

        [Fact]
        public async Task message_should_be_enqueued_to_and_dequeued_from_table()
        {
            // arrange
            var observer = new AwaitableObserver<IMessageDescriptor>();
            var @event = new SeatReserved( NewGuid(), Now, "John Doe" );
            var messageSent = @event.GetDescriptor();

            using ( MessageReceiver.Subscribe( observer ) )
            {
                // act
                await MessageSender.Send( messageSent, CancellationToken.None );
                var messageReceived = await observer.Received;

                // assert
                messageReceived.Should().BeEquivalentTo(
                    new
                    {
                        AggregateId = @event.AggregateId,
                        ReservationTime = @event.ReservationTime,
                        Attendee = "John Doe"
                    },
                    options => options.ExcludingMissingMembers() );
            }
        }

        [Fact]
        public async Task message_should_be_reX2Denqueued_when_an_observer_error_occurs()
        {
            // arrange
            var error = new Exception( "Test error" );
            var observer = new FailOnceObserver<IMessageDescriptor>( error );
            var @event = new SeatReserved( NewGuid(), Now, "John Doe" );
            var messageSent = @event.GetDescriptor();

            using ( MessageReceiver.Subscribe( observer ) )
            {
                // act
                await MessageSender.Send( messageSent, CancellationToken.None );
                var messageReceived = await observer.Received;

                // assert
                messageReceived.Should().BeEquivalentTo(
                    new
                    {
                        AggregateId = @event.AggregateId,
                        ReservationTime = @event.ReservationTime,
                        Attendee = "John Doe"
                    },
                    options => options.ExcludingMissingMembers() );
                observer.Should().BeEquivalentTo(
                    new
                    {
                        MessagesReceived = 2,
                        Error = error
                    },
                    options => options.ExcludingMissingMembers() );
            }
        }
    }
}