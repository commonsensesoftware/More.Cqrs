// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using Events;
    using More.Domain.Commands;
    using More.Domain.Reflection;
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents the base implementation for a saga.
    /// </summary>
    /// <typeparam name="TData">The type of saga data.</typeparam>
    [ContractClass( typeof( SagaContract<> ) )]
    [DebuggerDisplay( "{GetType().Name}, Version = {Version}, Id = {Data.Id}" )]
    public abstract class Saga<TData> : Aggregate, ISaga<TData> where TData : class, ISagaData
    {
        TData data;

        /// <summary>
        /// Initializes a new instance of the <see cref="Saga{TData}"/> class.
        /// </summary>
        protected Saga() { }

        /// <summary>
        /// Gets or sets the saga data.
        /// </summary>
        /// <value>The associated <typeparamref name="TData">saga data</typeparamref>.</value>
        /// <remarks>This property is assigned by the infrastructure and should never be explicitly set.</remarks>
        public TData Data
        {
            get => data ?? ( data = NewData() );
            set => data = value;
        }

        /// <summary>
        /// Gets a value indicating whether the saga is complete.
        /// </summary>
        /// <value>True if the saga is complete; otherwise, false.</value>
        public bool Completed { get; private set; }

        /// <summary>
        /// Gets the unique saga identifier.
        /// </summary>
        /// <value>The identifier of the saga.</value>
        public override Guid Id => Data.Id;

        /// <summary>
        /// Gets or sets the version of the saga.
        /// </summary>
        /// <value>The version of the saga.</value>
        /// <remarks>The version can be used for concurrency checks.</remarks>
        public override int Version
        {
            get => Data.Version;
            protected set => Data.Version = value;
        }

        /// <summary>
        /// Creates and returns a new snapshot of the saga.
        /// </summary>
        /// <returns>An opaque <see cref="ISnapshot{TKey}">snapshot</see> object for the saga based on its current state.</returns>
        public override ISnapshot<Guid> CreateSnapshot() => Data;

        /// <summary>
        /// Configures the correlation for the saga.
        /// </summary>
        /// <param name="correlator">The <see cref="SagaCorrelator{Data}">correlator</see> used to correlate the saga.</param>
        protected abstract void CorrelateUsing( SagaCorrelator<TData> correlator );

        /// <summary>
        /// Marks the saga as complete.
        /// </summary>
        protected virtual void MarkAsComplete() => Completed = true;

        /// <summary>
        /// Requests that the saga timeout when the specified event as been received.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="when">The <see cref="DateTimeOffset">date and time</see> when the timeout occurs.</param>
        /// <param name="event">The <typeparamref name="TEvent">event</typeparamref> generated when the timeout occurs.</param>
        /// <param name="context">The current <see cref="IMessageContext">message context</see>.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        protected Task RequestTimeout<TEvent>( DateTimeOffset when, TEvent @event, IMessageContext context ) where TEvent : class, IEvent
        {
            Arg.NotNull( @event, nameof( @event ) );
            Arg.NotNull( context, nameof( context ) );
            Contract.Ensures( Contract.Result<Task>() != null );

            EnsureSupportsTimeoutWhen( @event );
            var options = new PublishOptions().DoNotDeliverBefore( when );
            return context.Publish( @event, options );
        }

        /// <summary>
        /// Requests that the saga timeout when the specified event as been received.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="after">The <see cref="TimeSpan">amount of time</see> that should pass before the timeout occurs.</param>
        /// <param name="event">The <typeparamref name="TEvent">event</typeparamref> generated when the timeout occurs.</param>
        /// <param name="context">The current <see cref="IMessageContext">message context</see>.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        protected Task RequestTimeout<TEvent>( TimeSpan after, TEvent @event, IMessageContext context ) where TEvent : class, IEvent
        {
            Arg.NotNull( @event, nameof( @event ) );
            Arg.NotNull( context, nameof( context ) );
            Contract.Ensures( Contract.Result<Task>() != null );

            EnsureSupportsTimeoutWhen( @event );
            var options = new PublishOptions().DelayDeliveryBy( after );
            return context.Publish( @event, options );
        }

        /// <summary>
        /// Schedules a command for delivery.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="when">The <see cref="DateTimeOffset">date and time</see> when the command should be sent.</param>
        /// <param name="command">The <typeparamref name="TCommand">command</typeparamref> to be sent.</param>
        /// <param name="context">The current <see cref="IMessageContext">message context</see>.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        protected Task Schedule<TCommand>( DateTimeOffset when, TCommand command, IMessageContext context ) where TCommand : class, ICommand
        {
            Arg.NotNull( command, nameof( command ) );
            Arg.NotNull( context, nameof( context ) );
            Contract.Ensures( Contract.Result<Task>() != null );

            var options = new SendOptions().DoNotDeliverBefore( when );
            return context.Send( command, options );
        }

        /// <summary>
        /// Schedules a command for delivery.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="after">The <see cref="TimeSpan">amount of time</see> that should pass before the timeout occurs.</param>
        /// <param name="command">The <typeparamref name="TCommand">command</typeparamref> to be sent.</param>
        /// <param name="context">The current <see cref="IMessageContext">message context</see>.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        protected Task Schedule<TCommand>( TimeSpan after, TCommand command, IMessageContext context ) where TCommand : class, ICommand
        {
            Arg.NotNull( command, nameof( command ) );
            Arg.NotNull( context, nameof( context ) );
            Contract.Ensures( Contract.Result<Task>() != null );

            var options = new SendOptions().DelayDeliveryBy( after );
            return context.Send( command, options );
        }

#pragma warning disable CA1801 // Review unused parameters
        void EnsureSupportsTimeoutWhen<TEvent>( TEvent @event ) where TEvent : IEvent
#pragma warning restore CA1801 // Review unused parameters
        {
            if ( this is ITimeoutWhen<TEvent> )
            {
                return;
            }

            var message = SR.SagaDoesNotSupportTimeout.FormatDefault( GetType(), typeof( TEvent ), typeof( ITimeoutWhen<TEvent> ) );
            throw new NotSupportedException( message );
        }

        void ISaga<TData>.CorrelateUsing( ICorrelateSagaToMessage correlation )
        {
            Arg.NotNull( correlation, nameof( correlation ) );
            CorrelateUsing( new SagaCorrelator<TData>( correlation ) );
        }

        static TData NewData()
        {
            Contract.Ensures( Contract.Result<TData>() != null );

            try
            {
                return Activator.CreateInstance<TData>();
            }
            catch ( MissingMemberException )
            {
                return (TData) FormatterServices.GetUninitializedObject( typeof( TData ) );
            }
        }
    }
}