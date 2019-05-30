// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using Microsoft.Database.Isam;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Defines the behavior of an aggregate snapshot store backed by an ISAM database.
    /// </summary>
    /// <typeparam name="TKey">The type of aggregate key.</typeparam>
    public interface IIsamSnapshotStore<TKey>
    {
        /// <summary>
        /// Loads a snapshot for the specified aggregate identifier.
        /// </summary>
        /// <param name="database">The <see cref="IsamDatabase">database</see> used to load snapshots.</param>
        /// <param name="aggregateId">The identifier of the aggregate to load a snapshot for.</param>
        /// <returns>The loaded <see cref="IsamSnapshotDescriptor{TKey}">snapshot</see> or <c>null</c> if no snapshot is found.</returns>
        IsamSnapshotDescriptor<TKey> Load( IsamDatabase database, TKey aggregateId );

        /// <summary>
        /// Saves a snapshot.
        /// </summary>
        /// <param name="snapshots">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ISnapshot{TKey}">snapshots</see> to save.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        void Save( IEnumerable<ISnapshot<TKey>> snapshots, CancellationToken cancellationToken );
    }
}