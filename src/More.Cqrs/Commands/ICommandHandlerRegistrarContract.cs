// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Commands
{
    using System;
    using System.Diagnostics.Contracts;

    [ContractClassFor( typeof( ICommandHandlerRegistrar ) )]
    abstract class ICommandHandlerRegistrarContract : ICommandHandlerRegistrar
    {
        void ICommandHandlerRegistrar.Register<TCommand>( Func<IHandleCommand<TCommand>> handlerActivator )
        {
            Contract.Requires<ArgumentNullException>( handlerActivator != null, nameof( handlerActivator ) );
        }

        IHandleCommand<TCommand> ICommandHandlerRegistrar.ResolveFor<TCommand>( TCommand command )
        {
            Contract.Requires<ArgumentNullException>( command != null, nameof( command ) );
            Contract.Ensures( Contract.Result<IHandleCommand<TCommand>>() != null );
            return null;
        }
    }
}