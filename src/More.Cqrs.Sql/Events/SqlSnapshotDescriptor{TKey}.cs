// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;
    using System.IO;

    /// <summary>
    /// Represents a descriptor for an aggregate snapshot stored in a SQL database.
    /// </summary>
    /// <typeparam name="TKey">The type of aggregate key.</typeparam>
    public class SqlSnapshotDescriptor<TKey> : IDisposable
    {
        bool disposed;

        /// <summary>
        /// Finalizes an instance of the <see cref="SqlSnapshotDescriptor{TKey}"/> class.
        /// </summary>
        ~SqlSnapshotDescriptor() => Dispose( false );

        /// <summary>
        /// Gets or sets the aggregate identifier associated with the snapshot.
        /// </summary>
        /// <value>The associated aggregate identifier.</value>
        public TKey AggregateId { get; set; }

        /// <summary>
        /// Gets or sets the version of the aggregate when the snapshot was taken.
        /// </summary>
        /// <value>The version of the aggregate the snapshot corresponds to.</value>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the qualified name of the snapshot type.
        /// </summary>
        /// <value>The qualified snapshot type name.</value>
        public string SnapshotType { get; set; }

        /// <summary>
        /// Gets or sets the serialized snapshot.
        /// </summary>
        /// <value>The serialized snapshot data.</value>
        public Stream Snapshot { get; set; }

        /// <summary>
        /// Releases the managed and, optionally, the unmanaged resources used by the <see cref="SqlSnapshotDescriptor{TKey}"/> class.
        /// </summary>
        /// <param name="disposing">Indicates whether the object is being disposed.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;
            Snapshot?.Dispose();
        }

        /// <summary>
        /// Releases the managed resources used by the <see cref="SqlSnapshotDescriptor{TKey}"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }
    }
}