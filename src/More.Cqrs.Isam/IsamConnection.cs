// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using Microsoft.Database.Isam;
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Represents a connection for an ISAM database.
    /// </summary>
    public sealed class IsamConnection
    {
        static readonly object SyncRoot = new object();
        static readonly Dictionary<string, SharedIsamInstance> InstancePool = new Dictionary<string, SharedIsamInstance>( StringComparer.OrdinalIgnoreCase );
        readonly FileInfo file;

        /// <summary>
        /// Initializes a new instance of the <see cref="IsamConnection"/> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public IsamConnection( string connectionString )
        {
            Arg.NotNullOrEmpty( connectionString, nameof( connectionString ) );
            file = new FileInfo( connectionString );
        }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        /// <value>The name of the database the connection is for.</value>
        public string DatabaseName => file.FullName;

        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <returns>An <see cref="IsamInstance">instance</see> of the connected database.</returns>
        public IsamInstance Open()
        {
            lock ( SyncRoot )
            {
                if ( InstancePool.TryGetValue( DatabaseName, out var instance ) && instance.IncrementReferenceCount() )
                {
                    return instance;
                }

                var workingDirectory = file.DirectoryName + '\\';

                instance = new SharedIsamInstance( workingDirectory, workingDirectory, workingDirectory, "edb", "Isam", readOnly: false, pageSize: 4096 );
                instance.IsamSystemParameters.CreatePathIfNotExist = true;
                instance.IsamSystemParameters.CircularLog = true;
                InstancePool[DatabaseName] = instance;

                return instance;
            }
        }

        sealed class SharedIsamInstance : IsamInstance, IDisposable
        {
            readonly object syncRoot = new object();
            int referenceCount = 1;

            internal SharedIsamInstance(
                string checkpointFileDirectoryPath,
                string logfileDirectoryPath,
                string temporaryDatabaseFileDirectoryPath,
                string baseName,
                string eventSource,
                bool readOnly,
                int pageSize )
                : base( checkpointFileDirectoryPath, logfileDirectoryPath, temporaryDatabaseFileDirectoryPath, baseName, eventSource, readOnly, pageSize )
            {
            }

            public new void Dispose() => Dispose( true );

            internal bool IncrementReferenceCount()
            {
                lock ( syncRoot )
                {
                    if ( referenceCount == 0 )
                    {
                        return false;
                    }

                    ++referenceCount;
                }

                return true;
            }

            protected override void Dispose( bool disposing )
            {
                lock ( syncRoot )
                {
                    // we don't really want to dispose unless the reference count reaches zero
                    var disposed = referenceCount == 0;

                    if ( disposed )
                    {
                        return;
                    }

                    if ( --referenceCount == 0 )
                    {
                        base.Dispose( true );
#pragma warning disable CA1816
                        GC.SuppressFinalize( this );
#pragma warning restore CA1816
                    }
                }
            }
        }
    }
}