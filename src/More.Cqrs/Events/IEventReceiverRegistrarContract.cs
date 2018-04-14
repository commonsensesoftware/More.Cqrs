// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    [ContractClassFor( typeof( IEventReceiverRegistrar ) )]
    abstract class IEventReceiverRegistrarContract : IEventReceiverRegistrar
    {
        void IEventReceiverRegistrar.Register<TEvent>( Func<IReceiveEvent<TEvent>> receiverActivator )
        {
            Contract.Requires<ArgumentNullException>( receiverActivator != null, nameof( receiverActivator ) );
        }

        IEnumerable<IReceiveEvent<TEvent>> IEventReceiverRegistrar.ResolveFor<TEvent>( TEvent @event )
        {
            Contract.Requires<ArgumentNullException>( @event != null, nameof( @event ) );
            Contract.Ensures( Contract.Result<IEnumerable<IReceiveEvent<TEvent>>>() != null );
            return null;
        }
    }
}