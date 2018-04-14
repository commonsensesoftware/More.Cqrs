// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using Commands;
    using Events;
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior for message contexts.
    /// </summary>
    [ContractClass( typeof( IMessageContextContract ) )]
    public interface IMessageContext : IServiceProvider
    {
        /// <summary>
        /// Gets the cancellation token that can be used to cancel an operation.
        /// </summary>
        /// <value>The current <see cref="CancellationToken">cancellation token</see>.</value>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Sends a command through the pipeline.
        /// </summary>
        /// <param name="command">The <see cref="ICommand">command</see> to send.</param>
        /// <param name="options">The <see cref="SendOptions">options</see> associated with the <paramref name="command"/>.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        Task Send( ICommand command, SendOptions options );

        /// <summary>
        /// Publishes an event through the pipeline.
        /// </summary>
        /// <param name="event">The <see cref="IEvent">event</see> to publish.</param>
        /// <param name="options">The <see cref="PublishOptions">options</see> associated with the <paramref name="event"/>.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        Task Publish( IEvent @event, PublishOptions options );
    }
}