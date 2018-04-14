// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;

    /// <summary>
    /// Represents an aggregate snapshot store backed by a SQL database with snapshots having globally unique identifiers (GUID) for keys.
    /// </summary>
    public class SqlSnapshotStore : SqlSnapshotStore<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlSnapshotStore"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="SqlEventStoreConfiguration">configuration</see> used by the snapshot store.</param>
        public SqlSnapshotStore( SqlEventStoreConfiguration configuration ) : base( configuration ) { }
    }
}