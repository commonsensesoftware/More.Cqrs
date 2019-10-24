﻿// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

#pragma warning disable CA1716 // Identifiers should not match keywords

namespace More.Domain.Events
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior of an event publisher.
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publishes the specified event.
        /// </summary>
        /// <param name="event">The <see cref="IEvent">event</see> to publish.</param>
        /// <param name="options">The <see cref="PublishOptions">options</see> associated with the published event.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to can the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        Task Publish( IEvent @event, PublishOptions options, CancellationToken cancellationToken = default );
    }
}