// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Commands
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;
    using static System.Linq.Enumerable;

    /// <summary>
    /// Represents a registrar for <see cref="ICommand">commands</see> in the system.
    /// </summary>
    public class CommandRegistrar : ICommandHandlerRegistrar
    {
        readonly IServiceProvider serviceProvider;
        readonly ConcurrentDictionary<Type, List<Delegate>> commandHandlers = new ConcurrentDictionary<Type, List<Delegate>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRegistrar"/> class.
        /// </summary>
        public CommandRegistrar() : this( ServiceProvider.Default ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRegistrar"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider">service provider</see> used to dynamically resolve command handlers.</param>
        public CommandRegistrar( IServiceProvider serviceProvider )
        {
            Arg.NotNull( serviceProvider, nameof( serviceProvider ) );
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Registers a factory method used to resolve and activate a command handler for a given command.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="handlerActivator">The factory <see cref="Func{T}">method</see> used to activate the <see cref="IHandleCommand{T}">command handler</see>.</param>
        public virtual void Register<TCommand>( Func<IHandleCommand<TCommand>> handlerActivator ) where TCommand : class, ICommand
        {
            Arg.NotNull( handlerActivator, nameof( handlerActivator ) );
            commandHandlers.GetOrAdd( typeof( TCommand ), key => new List<Delegate>() ).Add( handlerActivator );
        }

        /// <summary>
        /// Resolves the command handlers for the specified command.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="command">The command to resolve the handlers for.</param>
        /// <returns>The <see cref="IHandleCommand{T}">command handler</see> registered for the specified <paramref name="command"/>.</returns>
        /// <exception cref="MissingCommandHandlerException">Occurs when a handler has not been registered for the specified <paramref name="command"/>.</exception>
        /// <exception cref="MultipleCommandHandlersException">Occurs when multiple handlers have been registered for the specified <paramref name="command"/>.</exception>
        public virtual IHandleCommand<TCommand> ResolveFor<TCommand>( TCommand command ) where TCommand : class, ICommand
        {
            Arg.NotNull( command, nameof( command ) );
            Contract.Ensures( Contract.Result<IEnumerable<IHandleCommand<TCommand>>>() != null );

            var commandType = command.GetType();
            var @explicit = commandHandlers.GetOrAdd( commandType, key => new List<Delegate>() ).Cast<Func<IHandleCommand<TCommand>>>().Select( activate => activate() );
            var @dynamic = (IEnumerable<IHandleCommand<TCommand>>) serviceProvider.GetService( typeof( IEnumerable<IHandleCommand<TCommand>> ) ) ?? Empty<IHandleCommand<TCommand>>();
            var handlers = @explicit.Union( @dynamic ).ToArray();

            switch ( handlers.Length )
            {
                case 0:
                    throw new MissingCommandHandlerException( SR.MissingCommandHandler.FormatDefault( commandType.Name ) );
                case 1:
                    return handlers[0];
            }

            var handlerTypeNames = new StringBuilder();

            handlerTypeNames.AppendLine();

            foreach ( var handler in handlers )
            {
                handlerTypeNames.AppendLine( handler.GetType().Name );
            }

            var message = SR.MultipleCommandHandlers.FormatDefault( commandType.Name, handlerTypeNames );
            throw new MultipleCommandHandlersException( message );
        }
    }
}