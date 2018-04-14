// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using Commands;
    using Events;
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    [ContractClassFor( typeof( IMessageContext ) )]
    abstract class IMessageContextContract : IMessageContext
    {
        CancellationToken IMessageContext.CancellationToken => default( CancellationToken );

        Task IMessageContext.Send( ICommand command, SendOptions options )
        {
            Contract.Requires<ArgumentNullException>( command != null, nameof( command ) );
            Contract.Requires<ArgumentNullException>( options != null, nameof( options ) );
            Contract.Ensures( Contract.Result<Task>() != null );
            return null;
        }

        Task IMessageContext.Publish( IEvent @event, PublishOptions options )
        {
            Contract.Requires<ArgumentNullException>( @event != null, nameof( @event ) );
            Contract.Requires<ArgumentNullException>( options != null, nameof( options ) );
            Contract.Ensures( Contract.Result<Task>() != null );
            return null;
        }

        object IServiceProvider.GetService( Type serviceType ) => null;
    }
}