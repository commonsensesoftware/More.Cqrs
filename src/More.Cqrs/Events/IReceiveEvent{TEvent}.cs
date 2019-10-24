// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

#pragma warning disable CA1716 // Identifiers should not match keywords

namespace More.Domain.Events
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior of an event receiver.
    /// </summary>
    /// <typeparam name="TEvent">The <see cref="Type">type</see> of <see cref="IEvent">event</see>.</typeparam>
    public interface IReceiveEvent<in TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Receives the specified event.
        /// </summary>
        /// <param name="event">The <typeparamref name="TEvent">event</typeparamref> to receive.</param>
        /// <param name="context">The current <see cref="IMessageContext">message context</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="ValueTask">task</see> representing the asynchronous operation.</returns>
        ValueTask Receive( TEvent @event, IMessageContext context, CancellationToken cancellationToken );
    }
}