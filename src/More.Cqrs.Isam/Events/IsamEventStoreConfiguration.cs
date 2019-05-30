// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using Microsoft.Database.Isam;
    using More.Domain.Messaging;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using static Microsoft.Database.Isam.ColumnFlags;
    using static Microsoft.Database.Isam.IndexFlags;
    using static Microsoft.Database.Isam.BoundCriteria;

    /// <summary>
    /// Represents the configuration for an ISAM database event store.
    /// </summary>
    public class IsamEventStoreConfiguration
    {
        readonly string connectionString;
        readonly IIsamMessageSerializerFactory serializerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="IsamEventStoreConfiguration"/> class.
        /// </summary>
        /// <param name="entityName">The name of the entity the event store is for.</param>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="tableName">The event store table name.</param>
        /// <param name="snapshots">The associated <see cref="IsamSnapshotConfiguration">snapshot configuration</see>.</param>
        /// <param name="messageTypeResolver">The <see cref="IMessageTypeResolver">message type resolver</see> used to deserialize messages.</param>
        /// <param name="serializerFactory">The <see cref="IIsamMessageSerializerFactory">factory</see> used to create new message serializers.</param>
        public IsamEventStoreConfiguration(
            string entityName,
            string connectionString,
            string tableName,
            IsamSnapshotConfiguration snapshots,
            IMessageTypeResolver messageTypeResolver,
            IIsamMessageSerializerFactory serializerFactory )
        {
            Arg.NotNullOrEmpty( entityName, nameof( entityName ) );
            Arg.NotNullOrEmpty( connectionString, nameof( connectionString ) );
            Arg.NotNull( snapshots, nameof( snapshots ) );
            Arg.NotNull( messageTypeResolver, nameof( messageTypeResolver ) );
            Arg.NotNull( serializerFactory, nameof( serializerFactory ) );

            EntityName = entityName;
            this.connectionString = connectionString;
            this.serializerFactory = serializerFactory;
            TableName = tableName;
            Snapshots = snapshots;
            MessageTypeResolver = messageTypeResolver;
            EventSerializer = serializerFactory.NewSerializer<IEvent>();
        }

        /// <summary>
        /// Gets the name of the entity the event store is for.
        /// </summary>
        /// <value>The name of the entity the event store is for.</value>
        /// <remarks>The entity name is the logical name of the entity stored in the event store. The name might be the same as the
        /// <see cref="TableName">table name</see> when Table-Per-Entity mapping is used. The name will be different when entities
        /// with the same table schema are stored in the same table. The specified name is also meant to be used as a key to
        /// uniquely identify the event store.</remarks>
        public string EntityName { get; }

        /// <summary>
        /// Gets the identifier of the event store table.
        /// </summary>
        /// <vaule>The identifier of the event store table.</vaule>
        public string TableName { get; }

        /// <summary>
        /// Gets the associated snapshot configuration.
        /// </summary>
        /// <value>The associated <see cref="IsamSnapshotConfiguration">snapshot configuration</see>.</value>
        public IsamSnapshotConfiguration Snapshots { get; }

        /// <summary>
        /// Gets the message type resolver used to deserialize messages.
        /// </summary>
        /// <value>The <see cref="IMessageTypeResolver">message type resolver</see> used to deserialize messages.</value>
        public IMessageTypeResolver MessageTypeResolver { get; }

        /// <summary>
        /// Gets the serializer used to serialize and deserialize events.
        /// </summary>
        /// <value>The <see cref="IIsamMessageSerializer{TMessage}">serializer</see> used to serialize and deserialize events.</value>
        public IIsamMessageSerializer<IEvent> EventSerializer { get; }

        /// <summary>
        /// Creates a new message serializer.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to create a serializer for.</typeparam>
        /// <returns>A new <see cref="IIsamMessageSerializer{TMessage}"/>.</returns>
        public IIsamMessageSerializer<TMessage> NewMessageSerializer<TMessage>() where TMessage : class, IMessage => serializerFactory.NewSerializer<TMessage>();

        /// <summary>
        /// Create a new database connection.
        /// </summary>
        /// <returns>A new, configured <see cref="IsamConnection">database connection</see>.</returns>
        public virtual IsamConnection CreateConnection() => new IsamConnection( connectionString );

        /// <summary>
        /// Creates the event store and, conditionally, the snapshot store tables.
        /// </summary>
        /// <param name="keyType">The type for the key column.</param>
        /// <remarks>The snapshot table is only created if snapshotting is supported.</remarks>
        public virtual void CreateTables( Type keyType )
        {
            Arg.NotNull( keyType, nameof( keyType ) );

            switch ( Type.GetTypeCode( keyType ) )
            {
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.String:
                    break;
                case TypeCode.Object:
                    if ( typeof( Guid ).Equals( keyType ) )
                    {
                        break;
                    }

                    goto default;
                default:
                    throw new ArgumentException( SR.InvalidKeyType.FormatDefault( keyType.Name ) );
            }

            var tables = new List<TableDefinition>( 2 ) { NewEventStoreTable( keyType ) };

            if ( Snapshots.Supported )
            {
                tables.Add( NewSnapshotTable( keyType ) );
            }

            var connection = CreateConnection();

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                using ( var transaction = new IsamTransaction( session ) )
                {
                    for ( var i = 0; i < tables.Count; i++ )
                    {
                        var table = tables[i];

                        if ( !database.Exists( table.Name ) )
                        {
                            database.CreateTable( table );
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Creates and returns a table definition for an event store.
        /// </summary>
        /// <param name="keyType">The type of key used in the event store.</param>
        /// <returns>A new <see cref="TableDefinition">table definition</see>.</returns>
        protected virtual TableDefinition NewEventStoreTable( Type keyType )
        {
            Arg.NotNull( keyType, nameof( keyType ) );

            var table = new TableDefinition( TableName );
            var columns = table.Columns;
            var primaryKeyIndex = new IndexDefinition( "PK_" + table.Name )
            {
                Flags = Primary | Unique | DisallowNull | DisallowTruncation,
                KeyColumns =
                {
                    new KeyColumn("AggregateId", isAscending: true),
                    new KeyColumn("Version", isAscending: true),
                    new KeyColumn("Sequence", isAscending: true),
                },
            };

            if ( keyType == typeof( string ) )
            {
                columns.Add( new ColumnDefinition( "AggregateId", keyType, Variable | NonNull ) { MaxLength = 128 } );
                primaryKeyIndex.CompareOptions = CompareOptions.OrdinalIgnoreCase;
            }
            else
            {
                columns.Add( new ColumnDefinition( "AggregateId", keyType, Fixed | NonNull ) );
            }

            columns.Add( new ColumnDefinition( "Version", typeof( int ), Fixed | NonNull ) );
            columns.Add( new ColumnDefinition( "Sequence", typeof( int ), Fixed | NonNull ) );
            columns.Add( new ColumnDefinition( "RecordedOn", typeof( DateTime ), Fixed | NonNull ) );
            columns.Add( new ColumnDefinition( "Type", typeof( string ), Variable | NonNull ) );
            columns.Add( new ColumnDefinition( "Revision", typeof( int ), Fixed | NonNull ) { DefaultValue = 1 } );
            columns.Add( new ColumnDefinition( "Message", typeof( byte[] ), NonNull ) );
            table.Indices.Add( primaryKeyIndex );

            return table;
        }

        /// <summary>
        /// Creates and returns a table definition for snapshots.
        /// </summary>
        /// <param name="keyType">The type of key used for snapshots.</param>
        /// <returns>A new <see cref="TableDefinition">table definition</see>.</returns>
        protected virtual TableDefinition NewSnapshotTable( Type keyType )
        {
            Arg.NotNull( keyType, nameof( keyType ) );

            var table = new TableDefinition( Snapshots.TableName );
            var columns = table.Columns;
            var primaryKeyIndex = new IndexDefinition( "PK_" + table.Name )
            {
                Flags = Primary | Unique | DisallowNull | DisallowTruncation,
                KeyColumns =
                {
                    new KeyColumn("AggregateId", isAscending: true),
                    new KeyColumn("Version", isAscending: false),
                },
            };

            if ( keyType == typeof( string ) )
            {
                columns.Add( new ColumnDefinition( "AggregateId", keyType, Variable | NonNull ) { MaxLength = 128 } );
                primaryKeyIndex.CompareOptions = CompareOptions.OrdinalIgnoreCase;
            }
            else
            {
                columns.Add( new ColumnDefinition( "AggregateId", keyType, Fixed | NonNull ) );
            }

            columns.Add( new ColumnDefinition( "Version", typeof( int ), Fixed | NonNull ) );
            columns.Add( new ColumnDefinition( "TakenOn", typeof( DateTime ), Fixed | NonNull ) );
            columns.Add( new ColumnDefinition( "Type", typeof( string ), Variable | NonNull ) );
            columns.Add( new ColumnDefinition( "Snapshot", typeof( byte[] ), NonNull ) );
            table.Indices.Add( primaryKeyIndex );

            return table;
        }

        /// <summary>
        /// Reads a list of events from the specified data reader.
        /// </summary>
        /// <param name="cursor">The <see cref="Cursor">cursor</see> to read from.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of the <see cref="IEvent">events</see> read.</returns>
        public virtual IReadOnlyList<IEvent> ReadEvents( Cursor cursor, CancellationToken cancellationToken )
        {
            Arg.NotNull( cursor, nameof( cursor ) );
            Contract.Ensures( Contract.Result<IReadOnlyList<IEvent>>() != null );

            var events = new List<IEvent>();

            foreach ( var record in cursor )
            {
                var messageType = (string) record["Type"][0];
                var revision = (int) record["Revision"][0];

                using ( var message = new MemoryStream( (byte[]) record["Message"][0], writable: false ) )
                {
                    events.Add( EventSerializer.Deserialize( messageType, revision, message ) );
                }

                cancellationToken.ThrowIfCancellationRequested();
            }

            return events;
        }

        /// <summary>
        /// Saves the specified event descriptor.
        /// </summary>
        /// <typeparam name="TKey">The type of key.</typeparam>
        /// <param name="eventDescriptor">The <see cref="EventDescriptor{TKey}">event descriptor</see> that
        /// describes the event being saved.</param>
        public virtual void SaveEvent<TKey>( EventDescriptor<TKey> eventDescriptor )
        {
            Arg.NotNull( eventDescriptor, nameof( eventDescriptor ) );

            var connection = CreateConnection();

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                using ( var cursor = database.OpenCursor( TableName ) )
                using ( var transaction = new IsamTransaction( session ) )
                {
                    SaveEvent( cursor, eventDescriptor );
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Saves the specified event descriptor using the provided cursor.
        /// </summary>
        /// <typeparam name="TKey">The type of key.</typeparam>
        /// <param name="cursor">The <see cref="Cursor">cursor</see> used to save the event.</param>
        /// <param name="eventDescriptor">The <see cref="EventDescriptor{TKey}">event descriptor</see> that
        /// describes the event being saved.</param>
        public virtual void SaveEvent<TKey>( Cursor cursor, EventDescriptor<TKey> eventDescriptor )
        {
            Arg.NotNull( cursor, nameof( cursor ) );
            Arg.NotNull( eventDescriptor, nameof( eventDescriptor ) );

            IMessageDescriptor messageDescriptor = eventDescriptor;

            cursor.BeginEditForInsert();

            var record = cursor.EditRecord;

            record["AggregateId"] = eventDescriptor.AggregateId;
            record["Version"] = eventDescriptor.Event.Version;
            record["Sequence"] = eventDescriptor.Event.Sequence;
            record["RecordedOn"] = DateTime.UtcNow;
            record["Type"] = messageDescriptor.MessageType;
            record["Revision"] = eventDescriptor.Event.Revision;
            record["Message"] = EventSerializer.Serialize( eventDescriptor.Event ).ToBytes();

            cursor.AcceptChanges();
        }

        /// <summary>
        /// Create a new cursor to load all of the events for an aggregate.
        /// </summary>
        /// <typeparam name="TKey">The type of key.</typeparam>
        /// <param name="database">The <see cref="IsamDatabase">database</see> used to load events.</param>
        /// <param name="aggregateId">The aggregate identifier the cursor is for.</param>
        /// <param name="version">The version of the aggregate to begin loading events from.</param>
        /// <returns>A new, configured <see cref="Cursor">cursor</see>.</returns>
        public virtual Cursor NewLoadEventsCursor<TKey>( IsamDatabase database, TKey aggregateId, int version )
        {
            Contract.Ensures( Contract.Result<Cursor>() != null );

            var firstKey = Key.Compose( aggregateId, version, 0 );
            var lastKey = Key.Compose( aggregateId, int.MaxValue, int.MaxValue );
            var cursor = database.OpenCursor( TableName );

            cursor.SetCurrentIndex( "PK_" + TableName );
            cursor.FindRecordsBetween( firstKey, Inclusive, lastKey, Inclusive );

            return cursor;
        }

        /// <summary>
        /// Creates a returns a new snapshot store backed by a SQL database.
        /// </summary>
        /// <typeparam name="TKey">The type of key used by the stored aggregate snapshots.</typeparam>
        /// <returns>A new, configured <see cref="IIsamSnapshotStore{TKey}">snapshot store</see>.</returns>
        public virtual IIsamSnapshotStore<TKey> CreateSnapshotStore<TKey>()
        {
            Contract.Ensures( Contract.Result<IIsamSnapshotStore<TKey>>() != null );

            if ( Snapshots.Supported )
            {
                return new IsamSnapshotStore<TKey>( this );
            }

            return SnapshotStore<TKey>.Unsupported;
        }

        /// <summary>
        /// Loads the most recent snapshot using the specified connection and aggregate identifier.
        /// </summary>
        /// <typeparam name="TKey">The type of aggregate identifier.</typeparam>
        /// <param name="aggregateId">The identifier of the aggregate to load the snapshot for.</param>
        /// <returns>The loaded <see cref="IsamSnapshotDescriptor{TKey}">snapshot</see> or <c>null</c> if no match is found.</returns>
        public virtual IsamSnapshotDescriptor<TKey> LoadSnapshot<TKey>( TKey aggregateId )
        {
            var connection = CreateConnection();

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                {
                    return LoadSnapshot( database, aggregateId );
                }
            }
        }

        /// <summary>
        /// Loads the most recent snapshot using the specified connection and aggregate identifier.
        /// </summary>
        /// <typeparam name="TKey">The type of aggregate identifier.</typeparam>
        /// <param name="database">The <see cref="IsamDatabase">database</see> used to load snapshots.</param>
        /// <param name="aggregateId">The identifier of the aggregate to load the snapshot for.</param>
        /// <returns>The loaded <see cref="IsamSnapshotDescriptor{TKey}">snapshot</see> or <c>null</c> if no match is found.</returns>
        public virtual IsamSnapshotDescriptor<TKey> LoadSnapshot<TKey>( IsamDatabase database, TKey aggregateId )
        {
            Arg.NotNull( database, nameof( database ) );

            var firstKey = Key.Compose( aggregateId, int.MaxValue );
            var lastKey = Key.Compose( aggregateId, 0 );
            var snapshot = default( IsamSnapshotDescriptor<TKey> );

            using ( var cursor = database.OpenCursor( Snapshots.TableName ) )
            {
                cursor.SetCurrentIndex( "PK_" + Snapshots.TableName );
                cursor.FindRecordsBetween( firstKey, Inclusive, lastKey, Inclusive );

                if ( cursor.MoveNext() )
                {
                    var record = cursor.Record;

                    snapshot = new IsamSnapshotDescriptor<TKey>()
                    {
                        AggregateId = aggregateId,
                        SnapshotType = (string) record["Type"],
                        Version = (int) record["Version"],
                        Snapshot = new MemoryStream( (byte[]) record["Snapshot"], writable: false ),
                    };
                }
            }

            return snapshot;
        }

        /// <summary>
        /// Saves the specified snapshot descriptor.
        /// </summary>
        /// <typeparam name="TKey">The type of key.</typeparam>
        /// <param name="snapshotDescriptor">The <see cref="EventDescriptor{TKey}">snapshot descriptor</see> that
        /// describes the snapshot being saved.</param>
        public virtual void SaveSnapshot<TKey>( IsamSnapshotDescriptor<TKey> snapshotDescriptor )
        {
            Arg.NotNull( snapshotDescriptor, nameof( snapshotDescriptor ) );

            var connection = CreateConnection();

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                using ( var cursor = database.OpenCursor( Snapshots.TableName ) )
                using ( var transaction = new IsamTransaction( session ) )
                {
                    SaveSnapshot( cursor, snapshotDescriptor );
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Saves the specified snapshot descriptor using the provided command.
        /// </summary>
        /// <typeparam name="TKey">The type of key.</typeparam>
        /// <param name="cursor">The <see cref="Cursor">cursor</see> used to save the snapshot.</param>
        /// <param name="snapshotDescriptor">The <see cref="EventDescriptor{TKey}">snapshot descriptor</see> that
        /// describes the snapshot being saved.</param>
        public virtual void SaveSnapshot<TKey>( Cursor cursor, IsamSnapshotDescriptor<TKey> snapshotDescriptor )
        {
            Arg.NotNull( cursor, nameof( cursor ) );
            Arg.NotNull( snapshotDescriptor, nameof( snapshotDescriptor ) );

            cursor.BeginEditForInsert();

            var record = cursor.EditRecord;

            record["AggregateId"] = snapshotDescriptor.AggregateId;
            record["Version"] = snapshotDescriptor.Version;
            record["TakenOn"] = DateTime.UtcNow;
            record["Type"] = snapshotDescriptor.SnapshotType;
            record["Snapshot"] = snapshotDescriptor.Snapshot.ToBytes();

            cursor.AcceptChanges();
        }

        sealed class SnapshotStore<TKey> : IIsamSnapshotStore<TKey>
        {
            SnapshotStore() { }

            internal static IIsamSnapshotStore<TKey> Unsupported { get; } = new SnapshotStore<TKey>();

            public IsamSnapshotDescriptor<TKey> Load( IsamDatabase database, TKey aggregateId ) => default;

            public void Save( IEnumerable<ISnapshot<TKey>> snapshots, CancellationToken cancellationToken ) { }
        }
    }
}