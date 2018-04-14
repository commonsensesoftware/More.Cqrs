// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;

    [ContractClassFor( typeof( IReceiveEvent<> ) )]
    abstract class IReceiveEventContract<TEvent> : IReceiveEvent<TEvent> where TEvent : IEvent
    {
        Task IReceiveEvent<TEvent>.Receive( TEvent @event, IMessageContext context )
        {
            Contract.Requires<ArgumentNullException>( @event != null, nameof( @event ) );
            Contract.Requires<ArgumentNullException>( context != null, nameof( context ) );
            Contract.Ensures( Contract.Result<Task>() != null );
            return null;
        }
    }
}