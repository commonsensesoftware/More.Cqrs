// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;

    /// <summary>
    /// Represents the configuration for a snapshot store backed by an ISAM database.
    /// </summary>
    public class IsamSnapshotConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsamSnapshotConfiguration"/> class.
        /// </summary>
        /// <param name="tableName">The snapshot store table name.</param>
        /// <param name="supported">Indicates whether snapshots are supported.</param>
        public IsamSnapshotConfiguration( string tableName, bool supported )
        {
            Arg.NotNullOrEmpty( tableName, nameof( tableName ) );

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
        /// <vaule>The identifier of the snapshot store table.</vaule>
        public string TableName { get; }
    }
}