// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Commands
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior of a command handler.
    /// </summary>
    /// <typeparam name="TCommand">The type of handled command.</typeparam>
    [ContractClass( typeof( IHandleCommandContract<> ) )]
    public interface IHandleCommand<in TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The <typeparamref name="TCommand">command</typeparamref> to handle.</param>
        /// <param name="context">The current <see cref="IMessageContext">message context</see>.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        Task Handle( TCommand command, IMessageContext context );
    }
}