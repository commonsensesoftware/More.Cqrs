// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Commands
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior of a command handler.
    /// </summary>
    /// <typeparam name="TCommand">The type of handled command.</typeparam>
    public interface IHandleCommand<in TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The <typeparamref name="TCommand">command</typeparamref> to handle.</param>
        /// <param name="context">The current <see cref="IMessageContext">message context</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="ValueTask">task</see> representing the asynchronous operation.</returns>
        ValueTask Handle( TCommand command, IMessageContext context, CancellationToken cancellationToken );
    }
}