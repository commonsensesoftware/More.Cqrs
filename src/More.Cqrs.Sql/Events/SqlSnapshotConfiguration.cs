// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;

    /// <summary>
    /// Represents the configuration for a snapshot store backed by a SQL database.
    /// </summary>
    public class SqlSnapshotConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlSnapshotConfiguration"/> class.
        /// </summary>
        /// <param name="tableName">The snapshot store table name.</param>
        /// <param name="supported">Indicates whether snapshots are supported.</param>
        public SqlSnapshotConfiguration( SqlIdentifier tableName, bool supported )
        {
            TableName = tableName;
            Supported = supported;
        }

        /// <summary>
        /// Gets a value indicating whether snapshots are supported.
        /// </summary>
        /// <value>True if snapshots are supported; otherwise, false.</value>
        public bool Supported { get; }

        /// <summary>
        /// Gets the identifier of the snapshot store table.
        /// </summary>
        /// <vaule>The <see cref="SqlIdentifier">identifier</see> of the snapshot store table.</vaule>
        public SqlIdentifier TableName { get; }
    }
}