// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using Commands;
    using Events;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides extension methods for the <see cref="IMessageContext"/> interface.
    /// </summary>
    public static class IMessageContextExtensions
    {
        /// <summary>
        /// Sends a command through the pipeline.
        /// </summary>
        /// <param name="context">The current message <see cref="IMessageContext">message context</see>.</param>
        /// <param name="command">The <see cref="ICommand">command</see> to send.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Send( this IMessageContext context, ICommand command )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( command, nameof( command ) );

            return context.Send( command, new SendOptions() );
        }

        /// <summary>
        /// Sends a sequence of commands through the pipeline.
        /// </summary>
        /// <param name="context">The current message <see cref="IMessageContext">message context</see>.</param>
        /// <param name="commands">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ICommand">commands</see> to send.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Send( this IMessageContext context, IEnumerable<ICommand> commands ) =>
            context.Send( commands, new SendOptions() );

        /// <summary>
        /// Sends a sequence of commands through the pipeline.
        /// </summary>
        /// <param name="context">The current message <see cref="IMessageContext">message context</see>.</param>
        /// <param name="commands">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ICommand">commands</see> to send.</param>
        /// <param name="options">The <see cref="SendOptions">options</see> associated with each <see cref="ICommand">command</see>.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Send( this IMessageContext context, IEnumerable<ICommand> commands, SendOptions options )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( commands, nameof( commands ) );
            Arg.NotNull( options, nameof( options ) );

            return Task.WhenAll( commands.Select( command => context.Send( command, options ) ) );
        }

        /// <summary>
        /// Publishes an event through the pipeline.
        /// </summary>
        /// <param name="context">The current message <see cref="IMessageContext">message context</see>.</param>
        /// <param name="event">The <see cref="IEvent">event</see> to publish.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Publish( this IMessageContext context, IEvent @event )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( @event, nameof( @event ) );

            return context.Publish( @event, new PublishOptions() );
        }

        /// <summary>
        /// Publishes a sequence of events through the pipeline.
        /// </summary>
        /// <param name="context">The current message <see cref="IMessageContext">message context</see>.</param>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> to publish.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Publish( this IMessageContext context, IEnumerable<IEvent> events ) =>
            context.Publish( events, new PublishOptions() );

        /// <summary>
        /// Publishes a sequence of events through the pipeline.
        /// </summary>
        /// <param name="context">The current message <see cref="IMessageContext">message context</see>.</param>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> to publish.</param>
        /// <param name="options">The <see cref="PublishOptions">options</see> associated with each <see cref="IEvent">event</see>.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Publish( this IMessageContext context, IEnumerable<IEvent> events, PublishOptions options )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( events, nameof( events ) );
            Arg.NotNull( options, nameof( options ) );

            return Task.WhenAll( events.Select( @event => context.Publish( @event, options ) ) );
        }
    }
}