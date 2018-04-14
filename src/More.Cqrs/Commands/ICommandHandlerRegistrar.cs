// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Commands
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines the behavior of a <see cref="ICommand">command</see> registrar.
    /// </summary>
    [ContractClass( typeof( ICommandHandlerRegistrarContract ) )]
    public interface ICommandHandlerRegistrar
    {
        /// <summary>
        /// Registers a factory method used to resolve and activate a command handler for a given command.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="handlerActivator">The factory <see cref="Func{T}">method</see> used to activate the <see cref="IHandleCommand{T}">command handler</see>.</param>
        void Register<TCommand>( Func<IHandleCommand<TCommand>> handlerActivator ) where TCommand : class, ICommand;

        /// <summary>
        /// Resolves the command handler for the specified command.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="command">The command to resolve the handler for.</param>
        /// <returns>The <see cref="IHandleCommand{T}">command handler</see> registered for the specified <paramref name="command"/>.</returns>
        /// <exception cref="MissingCommandHandlerException">Occurs when a handler has not been registered for the specified <paramref name="command"/>.</exception>
        /// <exception cref="MultipleCommandHandlersException">Occurs when multiple handlers have been registered for the specified <paramref name="command"/>.</exception>
        IHandleCommand<TCommand> ResolveFor<TCommand>( TCommand command ) where TCommand : class, ICommand;
    }
}