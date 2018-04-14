// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using More.Domain.Events;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;

    [ContractClassFor( typeof( IAggregate<> ) )]
    abstract class IAggregateContract<TKey> : IAggregate<TKey>
    {
        TKey IAggregate<TKey>.Id => default( TKey );

        int IAggregate<TKey>.Version => default( int );

        IReadOnlyList<IEvent> IAggregate<TKey>.UncommittedEvents
        {
            get
            {
                Contract.Ensures( Contract.Result<IReadOnlyList<IEvent>>() != null );
                return null;
            }
        }

        bool IChangeTracking.IsChanged => default( bool );

        void IAggregate<TKey>.ReplayAll( IEnumerable<IEvent> history )
        {
            Contract.Requires<ArgumentNullException>( history != null, nameof( history ) );
        }

        ISnapshot<TKey> IAggregate<TKey>.CreateSnapshot()
        {
            Contract.Ensures( Contract.Result<ISnapshot<TKey>>() != null );
            return null;
        }

        void IChangeTracking.AcceptChanges() { }
    }
}