// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using More.Domain.Commands;
    using More.Domain.Events;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior of a saga activator.
    /// </summary>
    [ContractClass( typeof( ISagaActivatorContract ) )]
    public interface ISagaActivator
    {
        /// <summary>
        /// Activates a saga using the provided data.
        /// </summary>
        /// <param name="data">The <see cref="ISagaData">state data</see> to activate the saga with.</param>
        /// <returns>The activated <see cref="ISagaInstance">saga instance</see>.</returns>
        ISagaInstance Activate( ISagaData data );

        /// <summary>
        /// Activates a saga from the specified command handler using the provided command.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="commandHandler">The <see cref="IHandleCommand{TCommand}">command handler</see> used to activate the saga.</param>
        /// <param name="command">The <typeparamref name="TCommand">command</typeparamref> used to activate the saga.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the activated <see cref="ISagaInstance">saga instance</see>.</returns>
        Task<ISagaInstance> Activate<TCommand>( IHandleCommand<TCommand> commandHandler, TCommand command, CancellationToken cancellationToken ) where TCommand : class, ICommand;

        /// <summary>
        /// Activates a saga from the specified event receiver using the provided event.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="eventReceiver">The <see cref="IReceiveEvent{TEvent}">event receiver</see> used to activate the saga.</param>
        /// <param name="event">The <typeparamref name="TEvent">event</typeparamref> used to activate the saga.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the activated <see cref="ISagaInstance">saga instance</see>.</returns>
        Task<ISagaInstance> Activate<TEvent>( IReceiveEvent<TEvent> eventReceiver, TEvent @event, CancellationToken cancellationToken ) where TEvent : class, IEvent;
    }
}