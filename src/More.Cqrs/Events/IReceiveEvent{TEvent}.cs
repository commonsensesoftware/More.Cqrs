// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior of an event receiver.
    /// </summary>
    /// <typeparam name="TEvent">The <see cref="Type">type</see> of <see cref="IEvent">event</see>.</typeparam>
    [ContractClass( typeof( IReceiveEventContract<> ) )]
    public interface IReceiveEvent<in TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Receives the specified event.
        /// </summary>
        /// <param name="event">The <typeparamref name="TEvent">event</typeparamref> to receive.</param>
        /// <param name="context">The current <see cref="IMessageContext">message context</see>.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        Task Receive( TEvent @event, IMessageContext context );
    }
}