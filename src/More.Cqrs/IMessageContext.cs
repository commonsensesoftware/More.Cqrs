﻿// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

#pragma warning disable CA1716 // Identifiers should not match keywords

namespace More.Domain
{
    using More.Domain.Commands;
    using More.Domain.Events;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior for message contexts.
    /// </summary>
    public interface IMessageContext : IServiceProvider
    {
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