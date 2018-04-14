// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Commands
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;

    [ContractClassFor( typeof( IHandleCommand<> ) )]
    abstract class IHandleCommandContract<TCommand> : IHandleCommand<TCommand> where TCommand : ICommand
    {
        Task IHandleCommand<TCommand>.Handle( TCommand command, IMessageContext context )
        {
            Contract.Requires<ArgumentNullException>( command != null, nameof( command ) );
            Contract.Requires<ArgumentNullException>( context != null, nameof( context ) );
            Contract.Ensures( Contract.Result<Task>() != null );
            return null;
        }
    }
}