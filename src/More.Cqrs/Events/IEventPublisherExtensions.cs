// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;
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
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Publish( this IEventPublisher eventPublisher, IEvent @event )
        {
            Arg.NotNull( eventPublisher, nameof( eventPublisher ) );
            Arg.NotNull( @event, nameof( @event ) );

            return eventPublisher.Publish( @event, new PublishOptions(), CancellationToken.None );
        }

        /// <summary>
        /// Publishes the specified event.
        /// </summary>
        /// <param name="eventPublisher">The extended <see cref="IEventPublisher"/>.</param>
        /// <param name="event">The <see cref="IEvent">event</see> to publish.</param>
        /// <param name="options">The <see cref="PublishOptions">options</see> associated with the publshed <see cref="IEvent">event</see>.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Publish( this IEventPublisher eventPublisher, IEvent @event, PublishOptions options )
        {
            Arg.NotNull( eventPublisher, nameof( eventPublisher ) );
            Arg.NotNull( @event, nameof( @event ) );
            Arg.NotNull( options, nameof( options ) );

            return eventPublisher.Publish( @event, options, CancellationToken.None );
        }

        /// <summary>
        /// Publishes the specified event.
        /// </summary>
        /// <param name="eventPublisher">The extended <see cref="IEventPublisher"/>.</param>
        /// <param name="event">The <see cref="IEvent">event</see> to publish.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Publish( this IEventPublisher eventPublisher, IEvent @event, CancellationToken cancellationToken )
        {
            Arg.NotNull( eventPublisher, nameof( eventPublisher ) );
            Arg.NotNull( @event, nameof( @event ) );

            return eventPublisher.Publish( @event, new PublishOptions(), cancellationToken );
        }

        /// <summary>
        /// Publishes the sequence of specified events.
        /// </summary>
        /// <param name="eventPublisher">The extended <see cref="IEventPublisher"/>.</param>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> to publish.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Publish( this IEventPublisher eventPublisher, IEnumerable<IEvent> events ) =>
            eventPublisher.Publish( events, new PublishOptions(), CancellationToken.None );

        /// <summary>
        /// Publishes the sequence of specified events.
        /// </summary>
        /// <param name="eventPublisher">The extended <see cref="IEventPublisher"/>.</param>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> to publish.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Publish( this IEventPublisher eventPublisher, IEnumerable<IEvent> events, CancellationToken cancellationToken ) =>
            eventPublisher.Publish( events, new PublishOptions(), cancellationToken );

        /// <summary>
        /// Publishes the sequence of specified events.
        /// </summary>
        /// <param name="eventPublisher">The extended <see cref="IEventPublisher"/>.</param>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> to publish.</param>
        /// <param name="options">The <see cref="PublishOptions">options</see> associated with each publshed <see cref="IEvent">event</see>.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Publish( this IEventPublisher eventPublisher, IEnumerable<IEvent> events, PublishOptions options ) =>
            eventPublisher.Publish( events, options, CancellationToken.None );

        /// <summary>
        /// Publishes the sequence of specified events.
        /// </summary>
        /// <param name="eventPublisher">The extended <see cref="IEventPublisher"/>.</param>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> to publish.</param>
        /// <param name="options">The <see cref="PublishOptions">options</see> associated with each publshed <see cref="IEvent">event</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Publish( this IEventPublisher eventPublisher, IEnumerable<IEvent> events, PublishOptions options, CancellationToken cancellationToken )
        {
            Arg.NotNull( eventPublisher, nameof( eventPublisher ) );
            Arg.NotNull( events, nameof( events ) );
            Arg.NotNull( options, nameof( options ) );

            return Task.WhenAll( events.Select( @event => eventPublisher.Publish( @event, options, cancellationToken ) ) );
        }
    }
}