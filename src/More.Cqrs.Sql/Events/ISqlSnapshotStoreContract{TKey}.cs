// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    [ContractClassFor( typeof( ISqlSnapshotStore<> ) )]
    abstract class ISqlSnapshotStoreContract<TKey> : ISqlSnapshotStore<TKey>
    {
        Task<SqlSnapshotDescriptor<TKey>> ISqlSnapshotStore<TKey>.Load( DbConnection connection, TKey aggregateId, CancellationToken cancellationToken )
        {
            Contract.Requires<ArgumentNullException>( connection != null, nameof( connection ) );
            Contract.Ensures( Contract.Result<Task<SqlSnapshotDescriptor<TKey>>>() != null );
            return null;
        }

        Task ISqlSnapshotStore<TKey>.Save( IEnumerable<ISnapshot<TKey>> snapshots, CancellationToken cancellationToken )
        {
            Contract.Requires<ArgumentNullException>( snapshots != null, nameof( snapshots ) );
            Contract.Ensures( Contract.Result<Task>() != null );
            return null;
        }
    }
}