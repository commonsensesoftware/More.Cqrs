// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;
    using System.Threading;

    /// <summary>
    /// Provides extension methods for the <see cref="IIsamSnapshotStore{TKey}"/> interface.
    /// </summary>
    public static class IIsamSnapshotStoreExtensions
    {
        /// <summary>
        /// Saves a snapshot.
        /// </summary>
        /// <typeparam name="TKey">The type of aggregate key.</typeparam>
        /// <param name="snapshotStore">The extended <see cref="IIsamSnapshotStore{TKey}"/>.</param>
        /// <param name="snapshot">The <see cref="ISnapshot{TKey}">snapshot</see> to save.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        public static void Save<TKey>( this IIsamSnapshotStore<TKey> snapshotStore, ISnapshot<TKey> snapshot, CancellationToken cancellationToken )
        {
            Arg.NotNull( snapshotStore, nameof( snapshotStore ) );
            Arg.NotNull( snapshot, nameof( snapshot ) );

            snapshotStore.Save( new[] { snapshot }, cancellationToken );
        }
    }
}