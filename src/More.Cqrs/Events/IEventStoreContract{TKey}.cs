// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    [ContractClassFor( typeof( IEventStore<> ) )]
    abstract class IEventStoreContract<TKey> : IEventStore<TKey>
    {
        Task<IEnumerable<IEvent>> IEventStore<TKey>.Load( TKey aggregateId, CancellationToken cancellationToken )
        {
            Contract.Ensures( Contract.Result<Task<IEnumerable<IEvent>>>() != null );
            return null;
        }

        Task IEventStore<TKey>.Save( TKey aggregateId, IEnumerable<IEvent> events, int expectedVersion, CancellationToken cancellationToken )
        {
            Contract.Requires<ArgumentNullException>( events != null, nameof( events ) );
            Contract.Ensures( Contract.Result<Task>() != null );
            return null;
        }
    }
}