// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain;
    using More.Domain.Commands;
    using More.Domain.Events;
    using More.Domain.Persistence;
    using More.Domain.Sagas;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an in-memory message bus.
    /// </summary>
    public class InMemoryMessageBus : MessageBus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryMessageBus"/> class.
        /// </summary>
        public InMemoryMessageBus() : base( NewDefaultConfiguration() ) =>
            PendingOperations = ( (InMemoryMessageBusConfiguration) Configuration ).PendingOperations;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryMessageBus"/> class.
        /// </summary>
        /// <param name="configuration">The associated <see cref="IMessageBusConfiguration">configuration</see>.</param>
        public InMemoryMessageBus( IMessageBusConfiguration configuration )
            : base( new InMemoryMessageBusConfiguration( configuration ) ) => PendingOperations = ( (InMemoryMessageBusConfiguration) Configuration ).PendingOperations;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryMessageBus"/> class.
        /// </summary>
        /// <param name="configuration">The associated <see cref="IMessageBusConfiguration">configuration</see>.</param>
        /// <param name="pendingOperations">The object used to track <see cref="PendingOperations">pending operations</see>.</param>
        public InMemoryMessageBus( IMessageBusConfiguration configuration, PendingOperations pendingOperations ) : base( configuration )
        {
            PendingOperations = pendingOperations;
        }

        /// <summary>
        /// Gets the object that tracks the number of pending operations.
        /// </summary>
        /// <value>A <see cref="PendingOperations">pending operations</see> object.</value>
        protected PendingOperations PendingOperations { get; }

        /// <summary>
        /// Releases the managed and, optionally, the unmanaged resources used by the <see cref="InMemoryMessageBus"/> class.
        /// </summary>
        /// <param name="disposing">Indicates whether the object is being disposed.</param>
        protected override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            PendingOperations.Dispose();
        }

        /// <summary>
        /// Flushes any pending messages within the message bus.
        /// </summary>
        /// <returns>A <see cref="Task">task</see> used to wait for all pending messages to be flushed from the bus.</returns>
        public override Task Flush() => PendingOperations.None();

        /// <summary>
        /// Sends a command through the bus.
        /// </summary>
        /// <param name="command">The <see cref="ICommand">command</see> to send.</param>
        /// <param name="options">The <see cref="SendOptions">options</see> associated with the <paramref name="command"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>The <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public override Task Send( ICommand command, SendOptions options, CancellationToken cancellationToken )
        {
            PendingOperations.Increment();
            return base.Send( command, options, cancellationToken );
        }

        /// <summary>
        /// Publishes an event through the bus.
        /// </summary>
        /// <param name="event">The <see cref="IEvent">event</see> to publish.</param>
        /// <param name="options">The <see cref="PublishOptions">options</see> associated with the <paramref name="event"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>The <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public override Task Publish( IEvent @event, PublishOptions options, CancellationToken cancellationToken )
        {
            PendingOperations.Increment();
            return base.Publish( @event, options, cancellationToken );
        }

        /// <summary>
        /// Occurs when a message is received by the bus.
        /// </summary>
        /// <param name="messageDescriptor">The received <see cref="IMessageDescriptor">message descriptor</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>The <see cref="Task">task</see> representing the asynchronous operation.</returns>
        protected override async Task OnMessageReceived( IMessageDescriptor messageDescriptor, CancellationToken cancellationToken )
        {
            Arg.NotNull( messageDescriptor, nameof( messageDescriptor ) );

            await base.OnMessageReceived( messageDescriptor, cancellationToken ).ConfigureAwait( false );
            PendingOperations.Decrement();
        }

        /// <summary>
        /// Occurs when a pipeline error is encountered.
        /// </summary>
        /// <param name="error">The <see cref="Exception">error</see> that was encountered.</param>
        protected override void OnPipelineError( Exception error )
        {
            Arg.NotNull( error, nameof( error ) );
            PendingOperations.Observe( error );
        }

        static IMessageBusConfiguration NewDefaultConfiguration()
        {
            var pendingOperations = new PendingOperations();
            var messageReceiver = new InMemoryMessageReceiver();
            var messageSender = new InMemoryMessageSender( messageReceiver, pendingOperations );
            var builder = new MessageBusConfigurationBuilder();

            builder.HasMessageReceiver( messageReceiver )
                   .HasMessageSender( messageSender )
                   .HasSagaStorage( new InMemorySagaStorage() )
                   .MapPersistenceWith( new PersistenceMapper() );

            return new InMemoryMessageBusConfiguration( builder.CreateConfiguration(), pendingOperations );
        }
    }
}