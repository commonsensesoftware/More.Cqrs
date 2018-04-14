// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using More.Domain.Events;

    [ContractClassFor( typeof( ISaga<> ) )]
    abstract class ISagaContract<TData> : ISaga<TData> where TData : class, ISagaData
    {
        TData ISaga<TData>.Data { get; set; }

        bool ISaga<TData>.Completed => default( bool );

        Guid IAggregate<Guid>.Id => default( Guid );

        int IAggregate<Guid>.Version => default( int );

        IReadOnlyList<IEvent> IAggregate<Guid>.UncommittedEvents => null;

        bool IChangeTracking.IsChanged => default( bool );

        void ISaga<TData>.CorrelateUsing( ICorrelateSagaToMessage correlation )
        {
            Contract.Requires<ArgumentNullException>( correlation != null, nameof( correlation ) );
        }

        void IAggregate<Guid>.ReplayAll( IEnumerable<IEvent> history ) { }

        ISnapshot<Guid> IAggregate<Guid>.CreateSnapshot() => null;

        void IChangeTracking.AcceptChanges() { }
    }
}