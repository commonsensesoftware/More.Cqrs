// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides extension methods for the <see cref="IEventPublisher"/> interface.
    /// </summary>
    public static class IEventPublisherExtensions
    {
        /// <summary>
        /// Publishes the specified event.
        /// </summary>
        /// <param name="eventPublisher">The extended <see cref="IEventPublisher"/>.</param>
        /// <param name="event">The <see cref="IEvent">event</see> to publish.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Publish( this IEventPublisher eventPublisher, IEvent @event, CancellationToken cancellationToken = default ) =>
            eventPublisher.Publish( @event, new PublishOptions(), cancellationToken );

        /// <summary>
        /// Publishes the sequence of specified events.
        /// </summary>
        /// <param name="eventPublisher">The extended <see cref="IEventPublisher"/>.</param>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> to publish.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Publish( this IEventPublisher eventPublisher, IEnumerable<IEvent> events, CancellationToken cancellationToken = default ) =>
            eventPublisher.Publish( events, new PublishOptions(), cancellationToken );

        /// <summary>
        /// Publishes the sequence of specified events.
        /// </summary>
        /// <param name="eventPublisher">The extended <see cref="IEventPublisher"/>.</param>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> to publish.</param>
        /// <param name="options">The <see cref="PublishOptions">options</see> associated with each published <see cref="IEvent">event</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Publish( this IEventPublisher eventPublisher, IEnumerable<IEvent> events, PublishOptions options, CancellationToken cancellationToken = default ) =>
            Task.WhenAll( events.Select( @event => eventPublisher.Publish( @event, options, cancellationToken ) ) );
    }
}