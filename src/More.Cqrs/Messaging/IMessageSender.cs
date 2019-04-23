// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior of a message sender.
    /// </summary>
    [ContractClass( typeof( IMessageSenderContract ) )]
    public interface IMessageSender
    {
        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="messages">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IMessageDescriptor">messages</see> to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        Task Send( IEnumerable<IMessageDescriptor> messages, CancellationToken cancellationToken );
    }
}