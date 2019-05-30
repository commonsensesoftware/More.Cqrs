// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;

    /// <summary>
    /// Represents an aggregate snapshot store backed by an ISAM database with snapshots having globally unique identifiers (GUID) for keys.
    /// </summary>
    public class IsamSnapshotStore : IsamSnapshotStore<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsamSnapshotStore"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IsamEventStoreConfiguration">configuration</see> used by the snapshot store.</param>
        public IsamSnapshotStore( IsamEventStoreConfiguration configuration ) : base( configuration ) { }
    }
}