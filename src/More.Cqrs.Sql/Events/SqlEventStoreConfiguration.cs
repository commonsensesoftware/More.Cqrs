// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Messaging;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Data.CommandBehavior;
    using static System.Threading.Tasks.Task;

    /// <summary>
    /// Represents the configuration for a SQL database event store.
    /// </summary>
    public class SqlEventStoreConfiguration
    {
        readonly string connectionString;
        readonly ISqlMessageSerializerFactory serializerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlEventStoreConfiguration"/> class.
        /// </summary>
        /// <param name="entityName">The name of the entity the event store is for.</param>
        /// <param name="providerFactory">The <see cref="DbProviderFactory">provider factory</see> for underlying database operations.</param>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="tableName">The event store table name.</param>
        /// <param name="snapshots">The associated <see cref="SqlSnapshotConfiguration">snapshot configuration</see>.</param>
        /// <param name="messageTypeResolver">The <see cref="IMessageTypeResolver">message type resolver</see> used to deserialize messages.</param>
        /// <param name="serializerFactory">The <see cref="ISqlMessageSerializerFactory">factory</see> used to create new message serializers.</param>
        public SqlEventStoreConfiguration(
            string entityName,
            DbProviderFactory providerFactory,
            string connectionString,
            SqlIdentifier tableName,
            SqlSnapshotConfiguration snapshots,
            IMessageTypeResolver messageTypeResolver,
            ISqlMessageSerializerFactory serializerFactory )
        {
            Arg.NotNullOrEmpty( entityName, nameof( entityName ) );
            Arg.NotNull( providerFactory, nameof( providerFactory ) );
            Arg.NotNullOrEmpty( connectionString, nameof( connectionString ) );
            Arg.NotNull( snapshots, nameof( snapshots ) );
            Arg.NotNull( messageTypeResolver, nameof( messageTypeResolver ) );
            Arg.NotNull( serializerFactory, nameof( serializerFactory ) );

            EntityName = entityName;
            ProviderFactory = providerFactory;
            this.connectionString = connectionString;
            this.serializerFactory = serializerFactory;
            TableName = tableName;
            Snapshots = snapshots;
            MessageTypeResolver = messageTypeResolver;
            EventSerializer = serializerFactory.NewSerializer<IEvent>();
            Sql = new SqlBuilder( this, snapshots );
        }

        SqlBuilder Sql { get; }

        /// <summary>
        /// Gets the database provider factory used by the configuration.
        /// </summary>
        /// <value>The configured <see cref="DbProviderFactory">database provider factory</see>.</value>
        protected DbProviderFactory ProviderFactory { get; }

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
        /// <vaule>The <see cref="SqlIdentifier">identifier</see> of the event store table.</vaule>
        public SqlIdentifier TableName { get; }

        /// <summary>
        /// Gets the associated snapshot configuration.
        /// </summary>
        /// <value>The associated <see cref="SqlSnapshotConfiguration">snapshot configuration</see>.</value>
        public SqlSnapshotConfiguration Snapshots { get; }

        /// <summary>
        /// Gets the message type resolver used to deserialize messages.
        /// </summary>
        /// <value>The <see cref="IMessageTypeResolver">message type resolver</see> used to deserialize messages.</value>
        public IMessageTypeResolver MessageTypeResolver { get; }

        /// <summary>
        /// Gets the serializer used to serialize and deserialize events.
        /// </summary>
        /// <value>The <see cref="ISqlMessageSerializer{TMessage}">serializer</see> used to serialize and deserialize events.</value>
        public ISqlMessageSerializer<IEvent> EventSerializer { get; }

        /// <summary>
        /// Creates a new message serializer.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to create a serializer for.</typeparam>
        /// <returns>A new <see cref="ISqlMessageSerializer{TMessage}"/>.</returns>
        public ISqlMessageSerializer<TMessage> NewMessageSerializer<TMessage>() where TMessage : class, IMessage => serializerFactory.NewSerializer<TMessage>();

        /// <summary>
        /// Create a new database connection.
        /// </summary>
        /// <returns>A new, configured <see cref="DbConnection">database connection</see>.</returns>
        public virtual DbConnection CreateConnection()
        {
            Contract.Ensures( Contract.Result<DbConnection>() != null );

            var connection = ProviderFactory.CreateConnection();
            connection.ConnectionString = connectionString;

            return connection;
        }

        /// <summary>
        /// Creates the event store and, conditionally, the snapshot store tables.
        /// </summary>
        /// <param name="keyType">The type for the key column.</param>
        /// <param name="cancellationToken">The optional <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        /// <remarks>The snapshot table is only created if snapshotting is supported.</remarks>
        public virtual Task CreateTables( Type keyType, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            Arg.NotNull( keyType, nameof( keyType ) );

            var dataType = default( string );

            switch ( Type.GetTypeCode( keyType ) )
            {
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    dataType = "SMALLINT";
                    break;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    dataType = "INT";
                    break;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    dataType = "BIGINT";
                    break;
                case TypeCode.String:
                    dataType = "NVARCHAR(128)";
                    break;
                case TypeCode.Object:
                    if ( typeof( Guid ).Equals( keyType ) )
                    {
                        dataType = "UNIQUEIDENTIFIER";
                        break;
                    }

                    goto default;
                default:
                    throw new ArgumentException( SR.InvalidKeyType.FormatDefault( keyType.Name ) );
            }

            return CreateTables( dataType, cancellationToken );
        }

        /// <summary>
        /// Creates the event store and, conditionally, the snapshot store tables.
        /// </summary>
        /// <param name="keyDataType">The SQL data type for the key column.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        /// <remarks>The snapshot table is only created if snapshotting is supported.</remarks>
        protected virtual async Task CreateTables( string keyDataType, CancellationToken cancellationToken )
        {
            Arg.NotNullOrEmpty( keyDataType, nameof( keyDataType ) );

            using ( var connection = CreateConnection() )
            {
                await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );

                using ( var transaction = connection.BeginTransaction() )
                using ( var command = connection.CreateCommand() )
                {
                    command.Transaction = transaction;
                    command.CommandText = Sql.EventStore.CreateSchema;
                    await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );

                    command.CommandText = Sql.EventStore.CreateTable( keyDataType );
                    await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );

                    if ( Snapshots.Supported )
                    {
                        command.CommandText = Sql.Snapshots.CreateSchema;
                        await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );

                        command.CommandText = Sql.Snapshots.CreateTable( keyDataType );
                        await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );
                    }

                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Reads a list of events from the specified data reader.
        /// </summary>
        /// <param name="dataReader">The <see cref="DbDataReader">data reader</see> to read from.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing a <see cref="IReadOnlyList{T}">read-only list</see>
        /// of the <see cref="IEvent">events</see> read.</returns>
        public virtual async Task<IReadOnlyList<IEvent>> ReadEvents( DbDataReader dataReader, CancellationToken cancellationToken )
        {
            Arg.NotNull( dataReader, nameof( dataReader ) );
            Contract.Ensures( Contract.Result<Task<IReadOnlyList<IEvent>>>() != null );

            var events = new List<IEvent>();

            if ( !await dataReader.ReadAsync( cancellationToken ).ConfigureAwait( false ) )
            {
                return events;
            }

            do
            {
                var messageType = dataReader.GetString( 0 );
                var revision = dataReader.GetInt32( 1 );

                using ( var message = dataReader.GetStream( 2 ) )
                {
                    events.Add( EventSerializer.Deserialize( messageType, revision, message ) );
                }
            }
            while ( await dataReader.ReadAsync( cancellationToken ).ConfigureAwait( false ) );

            return events;
        }

        /// <summary>
        /// Creates a new save command using the provided connection and aggregate identifier.
        /// </summary>
        /// <returns>A new, configured <see cref="DbCommand">database command</see>.</returns>
        public virtual DbCommand NewSaveEventCommand()
        {
            Contract.Ensures( Contract.Result<DbCommand>() != null );

            var command = ProviderFactory.CreateCommand();
            var parameter = command.CreateParameter();

            parameter.ParameterName = "AggregateId";
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.ParameterName = "Version";
            parameter.DbType = DbType.Int32;
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.ParameterName = "Sequence";
            parameter.DbType = DbType.Int32;
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.ParameterName = "Type";
            parameter.Size = 256;
            parameter.DbType = DbType.String;
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.ParameterName = "Revision";
            parameter.DbType = DbType.Int32;
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.ParameterName = "Message";
            parameter.DbType = DbType.Binary;
            command.Parameters.Add( parameter );

            command.CommandText = Sql.EventStore.Save;

            return command;
        }

        /// <summary>
        /// Saves the specified event descriptor.
        /// </summary>
        /// <typeparam name="TKey">The type of key.</typeparam>
        /// <param name="eventDescriptor">The <see cref="EventDescriptor{TKey}">event descriptor</see> that
        /// describes the event being saved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual async Task SaveEvent<TKey>( EventDescriptor<TKey> eventDescriptor, CancellationToken cancellationToken )
        {
            Arg.NotNull( eventDescriptor, nameof( eventDescriptor ) );

            using ( var connection = CreateConnection() )
            {
                await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );

                using ( var command = NewSaveEventCommand() )
                {
                    command.Connection = connection;
                    await SaveEvent( command, eventDescriptor, cancellationToken ).ConfigureAwait( false );
                }
            }
        }

        /// <summary>
        /// Saves the specified event descriptor using the provided command.
        /// </summary>
        /// <typeparam name="TKey">The type of key.</typeparam>
        /// <param name="command">The <see cref="DbCommand">database command</see> used to save the event.</param>
        /// <param name="eventDescriptor">The <see cref="EventDescriptor{TKey}">event descriptor</see> that
        /// describes the event being saved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual Task SaveEvent<TKey>( DbCommand command, EventDescriptor<TKey> eventDescriptor, CancellationToken cancellationToken )
        {
            Arg.NotNull( command, nameof( command ) );
            Arg.NotNull( eventDescriptor, nameof( eventDescriptor ) );

            IMessageDescriptor messageDescriptor = eventDescriptor;

            command.Parameters["AggregateId"].Value = eventDescriptor.AggregateId;
            command.Parameters["Version"].Value = eventDescriptor.Event.Version;
            command.Parameters["Sequence"].Value = eventDescriptor.Event.Sequence;
            command.Parameters["Type"].Value = messageDescriptor.MessageType;
            command.Parameters["Revision"].Value = eventDescriptor.Event.Revision;
            command.Parameters["Message"].Value = EventSerializer.Serialize( eventDescriptor.Event );

            return command.ExecuteNonQueryAsync( cancellationToken );
        }

        /// <summary>
        /// Create a new command to load all of the events for an aggregate.
        /// </summary>
        /// <typeparam name="TKey">The type of key.</typeparam>
        /// <param name="aggregateId">The aggregate identifier the command is for.</param>
        /// <param name="version">The version of the aggregate to begin loading events from.</param>
        /// <returns>A new, configured <see cref="DbCommand">database command</see>.</returns>
        public virtual DbCommand NewLoadEventsCommand<TKey>( TKey aggregateId, int version )
        {
            Contract.Ensures( Contract.Result<DbCommand>() != null );

            var command = NewEventQueryCommand( aggregateId, Sql.EventStore.Load );
            var parameter = command.CreateParameter();

            parameter.DbType = DbType.Int32;
            parameter.ParameterName = "Version";
            parameter.Value = version;
            command.Parameters.Add( parameter );

            return command;
        }

        /// <summary>
        /// Creates a returns a new snapshot store backed by a SQL database.
        /// </summary>
        /// <typeparam name="TKey">The type of key used by the stored aggregate snapshots.</typeparam>
        /// <returns>A new, configured <see cref="ISqlSnapshotStore{TKey}">snapshot store</see>.</returns>
        public virtual ISqlSnapshotStore<TKey> CreateSnapshotStore<TKey>()
        {
            Contract.Ensures( Contract.Result<ISqlSnapshotStore<TKey>>() != null );

            if ( Snapshots.Supported )
            {
                return new SqlSnapshotStore<TKey>( this );
            }

            return SnapshotStore<TKey>.Unsupported;
        }

        /// <summary>
        /// Loads the most recent snapshot using the specified connection and aggregate identifier.
        /// </summary>
        /// <typeparam name="TKey">The type of aggregate identifier.</typeparam>
        /// <param name="aggregateId">The identifier of the aggregate to load the snapshot for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>The <see cref="Task{TResult}">task</see> containing the loaded <see cref="SqlSnapshotDescriptor{TKey}">snapshot</see>
        /// or <c>null</c> if no match is found.</returns>
        public virtual async Task<SqlSnapshotDescriptor<TKey>> LoadSnapshot<TKey>( TKey aggregateId, CancellationToken cancellationToken )
        {
            using ( var connection = CreateConnection() )
            {
                return await LoadSnapshot( connection, aggregateId, cancellationToken ).ConfigureAwait( false );
            }
        }

        /// <summary>
        /// Loads the most recent snapshot using the specified connection and aggregate identifier.
        /// </summary>
        /// <typeparam name="TKey">The type of aggregate identifier.</typeparam>
        /// <param name="connection">The <see cref="DbConnection">connection</see> used to load the snapshot.</param>
        /// <param name="aggregateId">The identifier of the aggregate to load the snapshot for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>The <see cref="Task{TResult}">task</see> containing the loaded <see cref="SqlSnapshotDescriptor{TKey}">snapshot</see>
        /// or <c>null</c> if no match is found.</returns>
        public virtual async Task<SqlSnapshotDescriptor<TKey>> LoadSnapshot<TKey>( DbConnection connection, TKey aggregateId, CancellationToken cancellationToken )
        {
            Arg.NotNull( connection, nameof( connection ) );

            var snapshot = default( SqlSnapshotDescriptor<TKey> );

            using ( var command = connection.CreateCommand() )
            {
                var parameter = command.CreateParameter();

                parameter.ParameterName = "AggregateId";
                parameter.Value = aggregateId;
                command.Parameters.Add( parameter );
                command.CommandText = Sql.Snapshots.Load;

                using ( var reader = await command.ExecuteReaderAsync( SingleRow, cancellationToken ).ConfigureAwait( false ) )
                {
                    if ( await reader.ReadAsync( cancellationToken ).ConfigureAwait( false ) )
                    {
                        snapshot = new SqlSnapshotDescriptor<TKey>()
                        {
                            AggregateId = aggregateId,
                            SnapshotType = reader.GetString( 0 ),
                            Version = reader.GetInt32( 1 ),
                            Snapshot = reader.GetStream( 2 ),
                        };
                    }
                }
            }

            return snapshot;
        }

        /// <summary>
        /// Creates a new save command using the provided connection.
        /// </summary>
        /// <returns>A new, configured <see cref="DbCommand">database command</see>.</returns>
        public virtual DbCommand NewSaveSnapshotCommand()
        {
            Contract.Ensures( Contract.Result<DbCommand>() != null );

            var command = ProviderFactory.CreateCommand();
            var parameter = command.CreateParameter();

            parameter.ParameterName = "AggregateId";
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.ParameterName = "Version";
            parameter.DbType = DbType.Int32;
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.ParameterName = "Type";
            parameter.Size = 256;
            parameter.DbType = DbType.String;
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.ParameterName = "Snapshot";
            parameter.DbType = DbType.Binary;
            command.Parameters.Add( parameter );

            command.CommandText = Sql.Snapshots.Save;

            return command;
        }

        /// <summary>
        /// Saves the specified snapshot descriptor.
        /// </summary>
        /// <typeparam name="TKey">The type of key.</typeparam>
        /// <param name="snapshotDescriptor">The <see cref="EventDescriptor{TKey}">snapshot descriptor</see> that
        /// describes the snapshot being saved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual async Task SaveSnapshot<TKey>( SqlSnapshotDescriptor<TKey> snapshotDescriptor, CancellationToken cancellationToken )
        {
            Arg.NotNull( snapshotDescriptor, nameof( snapshotDescriptor ) );

            using ( var connection = CreateConnection() )
            {
                await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );

                using ( var command = NewSaveEventCommand() )
                {
                    command.Connection = connection;
                    await SaveSnapshot( command, snapshotDescriptor, cancellationToken ).ConfigureAwait( false );
                }
            }
        }

        /// <summary>
        /// Saves the specified snapshot descriptor using the provided command.
        /// </summary>
        /// <typeparam name="TKey">The type of key.</typeparam>
        /// <param name="command">The <see cref="DbCommand">database command</see> used to save the snapshot.</param>
        /// <param name="snapshotDescriptor">The <see cref="EventDescriptor{TKey}">snapshot descriptor</see> that
        /// describes the snapshot being saved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual Task SaveSnapshot<TKey>( DbCommand command, SqlSnapshotDescriptor<TKey> snapshotDescriptor, CancellationToken cancellationToken )
        {
            Arg.NotNull( command, nameof( command ) );
            Arg.NotNull( snapshotDescriptor, nameof( snapshotDescriptor ) );

            command.Parameters["AggregateId"].Value = snapshotDescriptor.AggregateId;
            command.Parameters["Version"].Value = snapshotDescriptor.Version;
            command.Parameters["Type"].Value = snapshotDescriptor.SnapshotType;
            command.Parameters["Snapshot"].Value = snapshotDescriptor.Snapshot;

            return command.ExecuteNonQueryAsync( cancellationToken );
        }

        DbCommand NewEventQueryCommand<TKey>( TKey primaryKey, string sql )
        {
            Contract.Requires( !string.IsNullOrEmpty( sql ) );
            Contract.Ensures( Contract.Result<DbCommand>() != null );

            var command = ProviderFactory.CreateCommand();
            var parameter = command.CreateParameter();

            parameter.ParameterName = "AggregateId";
            parameter.Value = primaryKey;
            command.Parameters.Add( parameter );
            command.CommandText = sql;

            return command;
        }

        sealed class SnapshotStore<TKey> : ISqlSnapshotStore<TKey>
        {
            SnapshotStore() { }

            internal static ISqlSnapshotStore<TKey> Unsupported { get; } = new SnapshotStore<TKey>();

            public Task<SqlSnapshotDescriptor<TKey>> Load( DbConnection connection, TKey aggregateId, CancellationToken cancellationToken ) => FromResult( default( SqlSnapshotDescriptor<TKey> ) );

            public Task Save( IEnumerable<ISnapshot<TKey>> snapshots, CancellationToken cancellationToken ) => CompletedTask;
        }
    }
}