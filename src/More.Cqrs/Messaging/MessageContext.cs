// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain.Commands;
    using More.Domain.Events;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a message context.
    /// </summary>
    public class MessageContext : IMessageContext
    {
        readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageContext"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider">service provider</see> associated with the context.</param>
        /// <param name="commandSender">The <see cref="ICommandSender">command sender</see> associated with the context.</param>
        /// <param name="eventPublisher">The <see cref="IEventPublisher">event publisher</see> associated with the context.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">cancellation token</see> for the context.</param>
        public MessageContext( IServiceProvider serviceProvider, ICommandSender commandSender, IEventPublisher eventPublisher, CancellationToken cancellationToken )
        {
            this.serviceProvider = serviceProvider;
            Commands = commandSender;
            Events = eventPublisher;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets the <see cref="ICommandSender">command sender</see> associated with the context.
        /// </summary>
        /// <value>The <see cref="ICommandSender">command sender</see> associated with the context.</value>
        protected ICommandSender Commands { get; }

        /// <summary>
        /// Gets the <see cref="IEventPublisher">event publisher</see> associated with the context.
        /// </summary>
        /// <value>The <see cref="IEventPublisher">event publisher</see> associated with the context.</value>
        protected IEventPublisher Events { get; }

        /// <summary>
        /// Gets the <see cref="CancellationToken">cancellation token</see> for the context.
        /// </summary>
        /// <value>The <see cref="CancellationToken">cancellation token</see> for the context.</value>
        protected CancellationToken CancellationToken { get; }

        /// <summary>
        /// Publishes an event via the current context.
        /// </summary>
        /// <param name="event">The <see cref="IEvent">event</see> to publish.</param>
        /// <param name="options">The associated <see cref="PublishOptions">options</see>.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public Task Publish( IEvent @event, PublishOptions options ) => Events.Publish( @event, options, CancellationToken );

        /// <summary>
        /// Sends a command via the current context.
        /// </summary>
        /// <param name="command">The <see cref="ICommand">command</see> to send.</param>
        /// <param name="options">The associated <see cref="SendOptions">options</see>.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public Task Send( ICommand command, SendOptions options ) => Commands.Send( command, options, CancellationToken );

        /// <summary>
        /// Gets a configured service matching the requested type.
        /// </summary>
        /// <param name="serviceType">The type of service requested.</param>
        /// <returns>The requested service or <c>null</c>.</returns>
        public object GetService( Type serviceType ) => serviceProvider.GetService( serviceType );
    }
}