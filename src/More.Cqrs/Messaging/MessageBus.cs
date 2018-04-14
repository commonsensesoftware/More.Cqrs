// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain;
    using More.Domain.Commands;
    using More.Domain.Events;
    using More.Domain.Sagas;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    /// <summary>
    /// Represents a message bus.
    /// </summary>
    public class MessageBus : ICommandSender, IEventPublisher, IDisposable
    {
        readonly MessageDispatcher dispatcher;
        readonly ICommandSender commandSender;
        bool disposed;
        IDisposable messageStream;

        /// <summary>
        /// Finalizes an instance of the <see cref="MessageBus"/> class.
        /// </summary>
        ~MessageBus() => Dispose( false );

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBus"/> class.
        /// </summary>
        /// <param name="configuration">The associated <see cref="IMessageBusConfiguration">configuration</see>.</param>
        public MessageBus( IMessageBusConfiguration configuration )
        {
            Arg.NotNull( configuration, nameof( configuration ) );

            Configuration = configuration;
            SagaActivator = new SagaActivator( configuration );
            dispatcher = new MessageDispatcher( configuration, SagaActivator );
            commandSender = new QueuedCommandSender( configuration.MessageSender );
        }

        /// <summary>
        /// Releases the managed and, optionally, the unmanaged resources used by the <see cref="MessageBus"/> class.
        /// </summary>
        /// <param name="disposing">Indicates whether the object is being disposed.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;

            if ( !disposing )
            {
                return;
            }

            messageStream?.Dispose();
        }

        /// <summary>
        /// Releases the managed resources used by the <see cref="MessageBus"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        /// <value>The current <see cref="IMessageBusConfiguration">configuration</see>.</value>
        public IMessageBusConfiguration Configuration { get; }

        /// <summary>
        /// Gets the configured message sender.
        /// </summary>
        /// <value>The configured <see cref="IMessageSender">message sender</see>.</value>
        protected IMessageSender MessageSender => Configuration.MessageSender;

        /// <summary>
        /// Gets the configured saga activator.
        /// </summary>
        /// <value>The configured <see cref="ISagaActivator">saga activator</see>.</value>
        protected ISagaActivator SagaActivator { get; }

        /// <summary>
        /// Gets the configured message receiver.
        /// </summary>
        /// <value>The configured <see cref="IMessageReceiver">message receiver</see>.</value>
        protected IMessageReceiver MessageReceiver => Configuration.MessageReceiver;

        /// <summary>
        /// Starts the message bus.
        /// </summary>
        /// <param name="observers">An additional, but optional, array of <see cref="IObserver{T}">observers</see>
        /// that listen to messages that pass through the bus.</param>
        public virtual void Start( params IObserver<IMessageDescriptor>[] observers )
        {
            var subscribers = new List<IObserver<IMessageDescriptor>>( observers ) { Observe.By( this ) };
            var observer = subscribers.Count == 1 ? subscribers[0] : new AggregateObserver( subscribers.ToArray() );

            messageStream = MessageReceiver.Subscribe( observer );
        }

        /// <summary>
        /// Subscribes the specified observer to begin listening to push-based messages received by the bus.
        /// </summary>
        /// <param name="observer">The <see cref="IObserver{T}">observer</see> used to listen to messages.</param>
        /// <returns>An opaque subscription that can be <see cref="IDisposable.Dispose">disposed</see> to stop listening for messages.</returns>
        public virtual IDisposable Listen( IObserver<IMessageDescriptor> observer ) => MessageReceiver.Subscribe( observer );

        /// <summary>
        /// Flushes any pending messages within the message bus.
        /// </summary>
        /// <returns>A <see cref="Task">task</see> used to wait for all pending messages to be flushed from the bus.</returns>
        public virtual Task Flush() => CompletedTask;

        /// <summary>
        /// Stops the message bus.
        /// </summary>
        /// <returns>A <see cref="Task">task</see> used to wait for the bus to stop.</returns>
        /// <remarks>The default implementation flushes all pending messages before the bus stops.</remarks>
        public virtual async Task Stop()
        {
            messageStream?.Dispose();
            await Flush().ConfigureAwait( false );
        }

        /// <summary>
        /// Sends a command through the bus.
        /// </summary>
        /// <param name="command">The <see cref="ICommand">command</see> to send.</param>
        /// <param name="options">The <see cref="SendOptions">options</see> associated with the <paramref name="command"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>The <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual Task Send( ICommand command, SendOptions options, CancellationToken cancellationToken )
        {
            Arg.NotNull( command, nameof( command ) );
            Arg.NotNull( options, nameof( options ) );

            return OnMessageReceived( command.GetDescriptor( options ), cancellationToken );
        }

        /// <summary>
        /// Publishes an event through the bus.
        /// </summary>
        /// <param name="event">The <see cref="IEvent">event</see> to publish.</param>
        /// <param name="options">The <see cref="PublishOptions">options</see> associated with the <paramref name="event"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>The <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual Task Publish( IEvent @event, PublishOptions options, CancellationToken cancellationToken )
        {
            Arg.NotNull( @event, nameof( @event ) );
            Arg.NotNull( options, nameof( options ) );

            return MessageSender.Send( @event.GetDescriptor( options ), cancellationToken );
        }

        /// <summary>
        /// Occurs when a message is received by the bus.
        /// </summary>
        /// <param name="messageDescriptor">The received <see cref="IMessageDescriptor">message descriptor</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>The <see cref="Task">task</see> representing the asynchronous operation.</returns>
        protected virtual async Task OnMessageReceived( IMessageDescriptor messageDescriptor, CancellationToken cancellationToken )
        {
            Arg.NotNull( messageDescriptor, nameof( messageDescriptor ) );

            var context = NewMessageContext( commandSender, this, cancellationToken );
            await dispatcher.Dispatch( messageDescriptor.Message, context ).ConfigureAwait( false );
        }

        /// <summary>
        /// Creates a new message context that can be provided to command and event handlers.
        /// </summary>
        /// <param name="commandSender">The <see cref="ICommandSender">command sender</see> used by the context.</param>
        /// <param name="eventPublisher">The <see cref="IEventPublisher">event publisher</see> used by the context.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">cancellation token</see> for the context.</param>
        /// <returns>A new <see cref="IMessageContext">message context</see>.</returns>
        protected virtual IMessageContext NewMessageContext( ICommandSender commandSender, IEventPublisher eventPublisher, CancellationToken cancellationToken )
        {
            Arg.NotNull( commandSender, nameof( commandSender ) );
            Arg.NotNull( eventPublisher, nameof( eventPublisher ) );
            Contract.Ensures( Contract.Result<IMessageContext>() != null );

            return new MessageContext( Configuration, commandSender, eventPublisher, cancellationToken );
        }

        /// <summary>
        /// Occurs when a pipeline error is encountered.
        /// </summary>
        /// <param name="error">The <see cref="Exception">error</see> that was encountered.</param>
        protected virtual void OnPipelineError( Exception error )
        {
            Arg.NotNull( error, nameof( error ) );
            error.Rethrow();
        }

        sealed class AggregateObserver : IObserver<IMessageDescriptor>
        {
            readonly IEnumerable<IObserver<IMessageDescriptor>> observers;

            internal AggregateObserver( IEnumerable<IObserver<IMessageDescriptor>> observers ) => this.observers = observers;

            public void OnCompleted()
            {
                foreach ( var observer in observers )
                {
                    observer.OnCompleted();
                }
            }

            public void OnError( Exception error )
            {
                foreach ( var observer in observers )
                {
                    observer.OnError( error );
                }
            }

            public void OnNext( IMessageDescriptor value )
            {
                foreach ( var observer in observers )
                {
                    observer.OnNext( value );
                }
            }
        }

        sealed class Observe : IObserver<IMessageDescriptor>, IDisposable
        {
            readonly MessageBus bus;
            bool disposed;
            CancellationTokenSource source = new CancellationTokenSource();

            ~Observe() => Dispose( false );

            Observe( MessageBus bus ) => this.bus = bus;

            internal static Observe By( MessageBus bus ) => new Observe( bus );

            public void OnCompleted() => source.Cancel();

            public void OnError( Exception error ) => bus.OnPipelineError( error );

            public async void OnNext( IMessageDescriptor value )
            {
                try
                {
                    await bus.OnMessageReceived( value, source.Token ).ConfigureAwait( false );
                }
                catch ( OperationCanceledException )
                {
                }
                catch ( Exception error )
                {
                    bus.OnPipelineError( error );
                }
            }

            void Dispose( bool disposing )
            {
                if ( disposed )
                {
                    return;
                }

                disposed = true;

                if ( disposing )
                {
                    source.Dispose();
                }
                else
                {
                    source?.Dispose();
                }
            }

            public void Dispose()
            {
                Dispose( true );
                GC.SuppressFinalize( this );
            }
        }

        sealed class QueuedCommandSender : ICommandSender
        {
            readonly IMessageSender sender;

            internal QueuedCommandSender( IMessageSender sender ) => this.sender = sender;

            public Task Send( ICommand command, SendOptions options, CancellationToken cancellationToken ) =>
                sender.Send( command.GetDescriptor( options ), cancellationToken );
        }
    }
}