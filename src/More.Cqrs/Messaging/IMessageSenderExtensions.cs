// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides extensions for the <see cref="IMessageSender"/> interface.
    /// </summary>
    public static class IMessageSenderExtensions
    {
        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="messageSender">The extended <see cref="IMessageSender"/>.</param>
        /// <param name="message">The <see cref="IMessageDescriptor">message</see> to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Send( this IMessageSender messageSender, IMessageDescriptor message, CancellationToken cancellationToken ) =>
            messageSender.Send( new[] { message }, cancellationToken );
    }
}