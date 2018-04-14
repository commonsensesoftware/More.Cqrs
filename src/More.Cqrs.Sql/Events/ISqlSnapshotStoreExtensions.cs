// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides extension methods for the <see cref="ISqlSnapshotStore{TKey}"/> interface.
    /// </summary>
    public static class ISqlSnapshotStoreExtensions
    {
        /// <summary>
        /// Saves a snapshot.
        /// </summary>
        /// <typeparam name="TKey">The type of aggregate key.</typeparam>
        /// <param name="snapshotStore">The extended <see cref="ISqlSnapshotStore{TKey}"/>.</param>
        /// <param name="snapshot">The <see cref="ISnapshot{TKey}">snapshot</see> to save.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public static Task Save<TKey>( this ISqlSnapshotStore<TKey> snapshotStore, ISnapshot<TKey> snapshot, CancellationToken cancellationToken )
        {
            Arg.NotNull( snapshotStore, nameof( snapshotStore ) );
            Arg.NotNull( snapshot, nameof( snapshot ) );

            return snapshotStore.Save( new[] { snapshot }, cancellationToken );
        }
    }
}