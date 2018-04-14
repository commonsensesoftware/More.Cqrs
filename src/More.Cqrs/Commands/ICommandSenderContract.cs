// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Commands
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    [ContractClassFor( typeof( ICommandSender ) )]
    abstract class ICommandSenderContract : ICommandSender
    {
        Task ICommandSender.Send( ICommand command, SendOptions options, CancellationToken cancellationToken )
        {
            Contract.Requires<ArgumentNullException>( command != null, nameof( command ) );
            Contract.Requires<ArgumentNullException>( options != null, nameof( options ) );
            Contract.Ensures( Contract.Result<Task>() != null );
            return null;
        }
    }
}