﻿// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides extension methods for the <see cref="ICommandSender"/> interface.
    /// </summary>
    public static class ICommandSenderExtensions
    {
        /// <summary>
        /// Sends a message within the system using the specified options.
        /// </summary>
        /// <param name="commandSender">The extended <see cref="ICommandSender"/>.</param>
        /// <param name="command">The <see cref="ICommand">command</see> to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see>representing the asynchronous operation.</returns>
        public static Task Send( this ICommandSender commandSender, ICommand command, CancellationToken cancellationToken = default ) =>
            commandSender.Send( command, new SendOptions(), cancellationToken );

        /// <summary>
        /// Sends a sequence of messages within the system using the specified options.
        /// </summary>
        /// <param name="commandSender">The extended <see cref="ICommandSender"/>.</param>
        /// <param name="commands">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ICommand">commands</see> to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see>representing the asynchronous operation.</returns>
        public static Task Send( this ICommandSender commandSender, IEnumerable<ICommand> commands, CancellationToken cancellationToken = default ) =>
            commandSender.Send( commands, new SendOptions(), cancellationToken );

        /// <summary>
        /// Sends a sequence of messages within the system using the specified options.
        /// </summary>
        /// <param name="commandSender">The extended <see cref="ICommandSender"/>.</param>
        /// <param name="commands">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ICommand">commands</see> to send.</param>
        /// <param name="options">The <see cref="SendOptions">options</see> used when sending each command.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see>representing the asynchronous operation.</returns>
        public static Task Send( this ICommandSender commandSender, IEnumerable<ICommand> commands, SendOptions options, CancellationToken cancellationToken = default ) =>
            Task.WhenAll( commands.Select( command => commandSender.Send( command, options, cancellationToken ) ) );
    }
}