// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Commands
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior of a command sender.
    /// </summary>
    [ContractClass( typeof( ICommandSenderContract ) )]
    public interface ICommandSender
    {
        /// <summary>
        /// Sends a message within the system using the specified options.
        /// </summary>
        /// <param name="command">The <see cref="ICommand">command</see> to send.</param>
        /// <param name="options">The <see cref="SendOptions">options</see> used when sending the command.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see>representing the asynchronous operation.</returns>
        Task Send( ICommand command, SendOptions options, CancellationToken cancellationToken );
    }
}