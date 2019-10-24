// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using More.Domain.Commands;
    using More.Domain.Events;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior of an object that activates saga instances.
    /// </summary>
    public interface ISagaInstanceActivator
    {
        /// <summary>
        /// Activates a saga using the provided metadata and clock.
        /// </summary>
        /// <param name="metadata">The <see cref="SagaMetadata">metadata</see> for the active saga.</param>
        /// <param name="clock">The <see cref="IClock">clock</see> associated with the active saga.</param>
        /// <returns>The activated <see cref="ISagaInstance">saga instance</see>.</returns>
        ISagaInstance Activate( SagaMetadata metadata, IClock clock );

        /// <summary>
        /// Activates a saga from the specified command handler using the provided metadata and clock.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="commandHandler">The <see cref="IHandleCommand{TCommand}">command handler</see> used to activate the saga.</param>
        /// <param name="metadata">The <see cref="SagaMetadata">metadata</see> for the active saga.</param>
        /// <param name="clock">The <see cref="IClock">clock</see> associated with the active saga.</param>
        /// <returns>The activated <see cref="ISagaInstance">saga instance</see>.</returns>
        ISagaInstance Activate<TCommand>( IHandleCommand<TCommand> commandHandler, SagaMetadata metadata, IClock clock ) where TCommand : notnull, ICommand;

        /// <summary>
        /// Activates a saga from the specified event receiver using the provided metadata and clock.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="eventReceiver">The <see cref="IReceiveEvent{TEvent}">event receiver</see> used to activate the saga.</param>
        /// <param name="metadata">The <see cref="SagaMetadata">metadata</see> for the active saga.</param>
        /// <param name="clock">The <see cref="IClock">clock</see> associated with the active saga.</param>
        /// <returns>The activated <see cref="ISagaInstance">saga instance</see>.</returns>
        ISagaInstance Activate<TEvent>( IReceiveEvent<TEvent> eventReceiver, SagaMetadata metadata, IClock clock ) where TEvent : notnull, IEvent;

        /// <summary>
        /// Gets the data for the active saga instance using the specified saga storage and message.
        /// </summary>
        /// <param name="instance">The active <see cref="ISagaInstance">saga instance</see> to get the data for.</param>
        /// <param name="store">The <see cref="IStoreSagaData">saga storage</see> used to retrieve the data.</param>
        /// <param name="message">The correlated message used to retrieve the data.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <see cref="SagaSearchResult">search result</see>.</returns>
        Task<SagaSearchResult> GetData( ISagaInstance instance, IStoreSagaData store, object message, CancellationToken cancellationToken );
    }
}