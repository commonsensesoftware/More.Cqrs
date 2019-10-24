// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Commands
{
    using System;

    /// <summary>
    /// Provides extension methods for the <see cref="IHandleCommand{T}"/> interface.
    /// </summary>
    public static class IHandleCommandExtensions
    {
        /// <summary>
        /// Returns a value indicating whether the command handler is a saga.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="commandHandler">The <see cref="IHandleCommand{T}">command handler</see> to evaluate.</param>
        /// <returns>True if the <paramref name="commandHandler">command handler</paramref> is a saga; otherwise, false.</returns>
        public static bool IsSaga<TCommand>( this IHandleCommand<TCommand> commandHandler ) where TCommand : ICommand => commandHandler.GetType().IsSaga();
    }
}