// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior of an aggregate snapshot store backed by a SQL database.
    /// </summary>
    /// <typeparam name="TKey">The type of aggregate key.</typeparam>
    public interface ISqlSnapshotStore<TKey> where TKey : notnull
    {
        /// <summary>
        /// Loads a snapshot for the specified aggregate identifier.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection">connection</see> used to load snapshots.</param>
        /// <param name="aggregateId">The identifier of the aggregate to load a snapshot for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the loaded <see cref="SqlSnapshotDescriptor{TKey}">snapshot</see>
        /// or <c>null</c> if no snapshot is found.</returns>
        Task<SqlSnapshotDescriptor<TKey>?> Load( DbConnection connection, TKey aggregateId, CancellationToken cancellationToken );

        /// <summary>
        /// Saves a snapshot.
        /// </summary>
        /// <param name="snapshots">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ISnapshot{TKey}">snapshots</see> to save.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        Task Save( IEnumerable<ISnapshot<TKey>> snapshots, CancellationToken cancellationToken );
    }
}