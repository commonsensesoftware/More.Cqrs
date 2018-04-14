namespace More.Domain.Messaging
{
    using FluentAssertions;
    using Moq;
    using More.Domain.Commands;
    using More.Domain.Events;
    using More.Domain.Persistence;
    using More.Domain.Sagas;
    using System;
    using Xunit;
    using static System.Linq.Enumerable;

    public class MessageBusConfigurationBuilderTest
    {
        [Fact]
        public void build_should_create_expected_configuration()
        {
            // arrange
            var serviceProvider = new Mock<IServiceProvider>().Object;
            var clock = new Mock<IClock>().Object;
            var persistenceMapper = new Mock<IMapPersistence>().Object;
            var messageSender = new Mock<IMessageSender>().Object;
            var messageReceiver = new Mock<IMessageReceiver>().Object;
            var commandHandlerRegistry = new Mock<ICommandHandlerRegistrar>().Object;
            var eventReceiverRegistry = new Mock<IEventReceiverRegistrar>().Object;
            var sagaStorage = new Mock<IStoreSagaData>().Object;
            var sagaMetadata = new SagaMetadataCollection( Empty<Type>() );
            var uniqueIdGenerator = new Mock<IUniqueIdGenerator>().Object;
            var builder = new MessageBusConfigurationBuilder();

            builder.HasServiceProvider( serviceProvider )
                   .UseClock( clock )
                   .MapPersistenceWith( persistenceMapper )
                   .HasMessageSender( messageSender )
                   .HasMessageReceiver( messageReceiver )
                   .HasCommandHandlerRegistrar( commandHandlerRegistry )
                   .HasEventReceiverRegistrar( eventReceiverRegistry )
                   .HasSagaStorage( sagaStorage )
                   .HasSagaMetadata( sagaMetadata )
                   .UseUniqueIdGenerator( uniqueIdGenerator );

            // act
            var configuration = builder.CreateConfiguration();

            // assert
            configuration.Should().BeEquivalentTo(
                new
                {
                    Clock = clock,
                    Persistence = persistenceMapper,
                    MessageSender = messageSender,
                    MessageReceiver = messageReceiver,
                    CommandHandlers = commandHandlerRegistry,
                    EventReceivers = eventReceiverRegistry,
                    Sagas = new SagaConfiguration( sagaStorage, sagaMetadata ),
                    UniqueIdGenerator = uniqueIdGenerator
                } );
        }
    }
}