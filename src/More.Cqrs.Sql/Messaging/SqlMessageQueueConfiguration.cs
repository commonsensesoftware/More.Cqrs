// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Data.CommandBehavior;

    /// <summary>
    /// Represents the configuration for a SQL database message queue.
    /// </summary>
    public class SqlMessageQueueConfiguration
    {
        readonly string connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlMessageQueueConfiguration"/> class.
        /// </summary>
        /// <param name="clock">The <see cref="IClock">clock</see> used for temporal calculations.</param>
        /// <param name="providerFactory">The <see cref="DbProviderFactory">provider factory</see> for underlying database operations.</param>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="messageQueueTableName">The <see cref="SqlIdentifier">identifier</see> of the message queue table.</param>
        /// <param name="subscriptionTableName">The <see cref="SqlIdentifier">identifier</see> of the subscription table.</param>
        /// <param name="subscriptionQueueTableName">The <see cref="SqlIdentifier">identifier</see> of the subscription queue table.</param>
        /// <param name="messageTypeResolver">The <see cref="IMessageTypeResolver">message type resolver</see> used to deserialize messages.</param>
        /// <param name="serializerFactory">The <see cref="ISqlMessageSerializerFactory">factory</see> used to create new message serializers.</param>
        public SqlMessageQueueConfiguration(
            IClock clock,
            DbProviderFactory providerFactory,
            string connectionString,
            SqlIdentifier messageQueueTableName,
            SqlIdentifier subscriptionTableName,
            SqlIdentifier subscriptionQueueTableName,
            IMessageTypeResolver messageTypeResolver,
            ISqlMessageSerializerFactory serializerFactory )
        {
            Arg.NotNull( providerFactory, nameof( providerFactory ) );
            Arg.NotNullOrEmpty( connectionString, nameof( connectionString ) );
            Arg.NotNull( messageTypeResolver, nameof( messageTypeResolver ) );
            Arg.NotNull( serializerFactory, nameof( serializerFactory ) );

            Clock = clock;
            ProviderFactory = providerFactory;
            this.connectionString = connectionString;
            MessageQueueTableName = messageQueueTableName;
            SubscriptionTableName = subscriptionTableName;
            SubscriptionQueueTableName = subscriptionQueueTableName;
            MessageTypeResolver = messageTypeResolver;
            MessageSerializer = serializerFactory.NewSerializer<IMessage>();
            Sql = new SqlBuilder( messageQueueTableName, subscriptionTableName, subscriptionQueueTableName );
        }

        SqlBuilder Sql { get; }

        /// <summary>
        /// Gets the database provider factory used by the configuration.
        /// </summary>
        /// <value>The configured <see cref="DbProviderFactory">database provider factory</see>.</value>
        protected DbProviderFactory ProviderFactory { get; }

        /// <summary>
        /// Gets the configured clock.
        /// </summary>
        /// <value>The configured <see cref="IClock">clock</see>.</value>
        public IClock Clock { get; }

        /// <summary>
        /// Gets the identifier of the message queue table.
        /// </summary>
        /// <vaule>The <see cref="SqlIdentifier">identifier</see> of the message queue table.</vaule>
        public SqlIdentifier MessageQueueTableName { get; }

        /// <summary>
        /// Gets the identifier of the subscription table.
        /// </summary>
        /// <vaule>The <see cref="SqlIdentifier">identifier</see> of the subscription table.</vaule>
        public SqlIdentifier SubscriptionTableName { get; }

        /// <summary>
        /// Gets the identifier of the subscription queue table.
        /// </summary>
        /// <vaule>The <see cref="SqlIdentifier">identifier</see> of the subscription queue table.</vaule>
        public SqlIdentifier SubscriptionQueueTableName { get; }

        /// <summary>
        /// Gets the name of the schema for the event store table.
        /// </summary>
        /// <value>The event store table schema name.</value>
        public string SchemaName { get; }

        /// <summary>
        /// Gets the name of the event store table.
        /// </summary>
        /// <value>The event store table name.</value>
        public string TableName { get; }

        /// <summary>
        /// Gets the message type resolver used to deserialize messages.
        /// </summary>
        /// <value>The <see cref="IMessageTypeResolver">message type resolver</see> used to deserialize messages.</value>
        public IMessageTypeResolver MessageTypeResolver { get; }

        /// <summary>
        /// Gets the serializer used to serialize and deserialize messages.
        /// </summary>
        /// <value>The <see cref="ISqlMessageSerializer{TMessage}">serializer</see> used to serialize and deserialize messages.</value>
        public ISqlMessageSerializer<IMessage> MessageSerializer { get; }

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
        /// Creates the message queue SQL table.
        /// </summary>
        /// <param name="cancellationToken">The optional <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual async Task CreateTables( CancellationToken cancellationToken = default( CancellationToken ) )
        {
            using ( var connection = CreateConnection() )
            {
                await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );

                using ( var transaction = connection.BeginTransaction() )
                using ( var command = connection.CreateCommand() )
                {
                    command.Transaction = transaction;
                    command.CommandText = Sql.MessageQueue.CreateSchema;
                    await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );

                    command.CommandText = Sql.MessageQueue.CreateTable;
                    await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );

                    command.CommandText = Sql.MessageQueue.CreateTrigger;
                    await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );

                    command.CommandText = Sql.MessageQueue.CreateIndex;
                    await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );

                    if ( Sql.MessageQueue.CreateSchema != Sql.Subscription.CreateSchema )
                    {
                        command.CommandText = Sql.Subscription.CreateSchema;
                        await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );
                    }

                    command.CommandText = Sql.Subscription.CreateTable;
                    await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );

                    command.CommandText = Sql.Subscription.CreateTrigger;
                    await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );

                    if ( Sql.MessageQueue.CreateSchema != Sql.SubscriptionQueue.CreateSchema &&
                         Sql.Subscription.CreateSchema != Sql.SubscriptionQueue.CreateSchema )
                    {
                        command.CommandText = Sql.SubscriptionQueue.CreateSchema;
                        await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );
                    }

                    command.CommandText = Sql.SubscriptionQueue.CreateTable;
                    await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );

                    command.CommandText = Sql.SubscriptionQueue.CreateIndex;
                    await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );

                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Creates a message queue subscription.
        /// </summary>
        /// <param name="subscriptionId">The associated subscription identifier.</param>
        /// <param name="creationTime">The <see cref="DateTimeOffset">date and time</see> when the subscription is to be created.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual async Task CreateSubscription( Guid subscriptionId, DateTimeOffset creationTime, CancellationToken cancellationToken )
        {
            using ( var connection = CreateConnection() )
            {
                await CreateSubscription( connection, subscriptionId, creationTime, cancellationToken ).ConfigureAwait( false );
            }
        }

        /// <summary>
        /// Creates a message queue subscription using the specified connection.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection">connection</see> to create a subscription with.</param>
        /// <param name="subscriptionId">The associated subscription identifier.</param>
        /// <param name="creationTime">The <see cref="DateTimeOffset">date and time</see> when the subscription is to be created.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual async Task CreateSubscription( DbConnection connection, Guid subscriptionId, DateTimeOffset creationTime, CancellationToken cancellationToken )
        {
            Arg.NotNull( connection, nameof( connection ) );
            Contract.Ensures( Contract.Result<Task>() != null );

            using ( var command = connection.CreateCommand() )
            {
                var parameter = command.CreateParameter();

                parameter.DbType = DbType.Guid;
                parameter.ParameterName = "SubscriptionId";
                parameter.Value = subscriptionId;
                command.Parameters.Add( parameter );

                parameter = command.CreateParameter();
                parameter.DbType = DbType.DateTime2;
                parameter.ParameterName = "CreationTime";
                parameter.Value = creationTime.UtcDateTime;
                command.Parameters.Add( parameter );

                command.CommandText = Sql.Subscription.CreateSubscription;

                await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );
            }
        }

        /// <summary>
        /// Deletes a message queue subscription using the specified subscription identifier.
        /// </summary>
        /// <param name="subscriptionId">The associated subscription identifier.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual async Task DeleteSubscription( Guid subscriptionId, CancellationToken cancellationToken )
        {
            Contract.Ensures( Contract.Result<Task>() != null );

            using ( var connection = CreateConnection() )
            {
                await DeleteSubscription( connection, subscriptionId, cancellationToken ).ConfigureAwait( false );
            }
        }

        /// <summary>
        /// Deletes a message queue subscription using the specified connection and subscription identifier.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection">connection</see> to delete a subscription with.</param>
        /// <param name="subscriptionId">The associated subscription identifier.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual async Task DeleteSubscription( DbConnection connection, Guid subscriptionId, CancellationToken cancellationToken )
        {
            Arg.NotNull( connection, nameof( connection ) );
            Contract.Ensures( Contract.Result<Task>() != null );

            using ( var command = connection.CreateCommand() )
            {
                var parameter = command.CreateParameter();

                parameter.DbType = DbType.Guid;
                parameter.ParameterName = "SubscriptionId";
                parameter.Value = subscriptionId;
                command.Parameters.Add( parameter );

                command.CommandText = Sql.Subscription.DeleteSubscription;

                await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );
            }
        }

        /// <summary>
        /// Creates and returns a new command to enqueue a message.
        /// </summary>
        /// <param name="newMessage">Indicates whether the command is for a brand new message.
        /// The default value is true.</param>
        /// <returns>A new, configured <see cref="DbCommand">command</see> that can be used to enqueue a message.</returns>
        public virtual DbCommand NewEnqueueCommand( bool newMessage = true )
        {
            Contract.Ensures( Contract.Result<DbCommand>() != null );

            var command = ProviderFactory.CreateCommand();
            var parameter = command.CreateParameter();

            parameter.DbType = DbType.DateTime2;
            parameter.ParameterName = "EnqueueTime";
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.DbType = DbType.DateTime2;
            parameter.ParameterName = "DueTime";
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.DbType = DbType.String;
            parameter.Size = 256;
            parameter.ParameterName = "Type";
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.DbType = DbType.Int32;
            parameter.ParameterName = "Revision";
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.DbType = DbType.Binary;
            parameter.ParameterName = "Message";
            command.Parameters.Add( parameter );

            if ( newMessage )
            {
                command.CommandText = Sql.MessageQueue.Enqueue;
            }
            else
            {
                command.CommandText = Sql.SubscriptionQueue.Enqueue;

                parameter = command.CreateParameter();
                parameter.DbType = DbType.Guid;
                parameter.ParameterName = "SubscriptionId";
                command.Parameters.Add( parameter );

                parameter = command.CreateParameter();
                parameter.DbType = DbType.Guid;
                parameter.ParameterName = "MessageId";
                command.Parameters.Add( parameter );

                parameter = command.CreateParameter();
                parameter.DbType = DbType.Int32;
                parameter.ParameterName = "DequeueAttempts";
                command.Parameters.Add( parameter );
            }

            return command;
        }

        /// <summary>
        /// Enqueues the specified item using the provided connection.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection">connection</see> to enqueue an item with.</param>
        /// <param name="item">The <see cref="SqlMessageQueueItem">item</see> to enqueue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual async Task Enqueue( DbConnection connection, SqlMessageQueueItem item, CancellationToken cancellationToken )
        {
            Arg.NotNull( connection, nameof( connection ) );
            Arg.NotNull( item, nameof( item ) );

            using ( var command = NewEnqueueCommand( newMessage: item.DequeueAttempts == 0 ) )
            {
                command.Connection = connection;

                if ( command.Transaction == null )
                {
                    command.Transaction = item.Transaction;
                }

                await Enqueue( command, item, cancellationToken ).ConfigureAwait( false );
            }
        }

        /// <summary>
        /// Enqueues the specified item using the provided command.
        /// </summary>
        /// <param name="command">The <see cref="DbCommand">command</see> to enqueue an item with.</param>
        /// <param name="item">The <see cref="SqlMessageQueueItem">item</see> to enqueue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual async Task Enqueue( DbCommand command, SqlMessageQueueItem item, CancellationToken cancellationToken )
        {
            Arg.NotNull( command, nameof( command ) );
            Arg.NotNull( item, nameof( item ) );

            command.Parameters["EnqueueTime"].Value = item.EnqueueTime;
            command.Parameters["DueTime"].Value = item.DueTime;
            command.Parameters["Type"].Value = item.MessageType;
            command.Parameters["Revision"].Value = item.Revision;
            command.Parameters["Message"].Value = item.Message;

            if ( item.DequeueAttempts > 0 )
            {
                command.Parameters["SubscriptionId"].Value = item.SubscriptionId;
                command.Parameters["MessageId"].Value = item.MessageId;
                command.Parameters["DequeueAttempts"].Value = item.DequeueAttempts;
            }

            await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );
        }

        /// <summary>
        /// Dequeues a single item using the specified connection.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection">connection</see> to dequeue an item with.</param>
        /// <param name="subscriptionId">The associated subscription identifier.</param>
        /// <param name="dueTime">The due <see cref="DateTimeOffset">date and time</see> for the next item to be dequeued.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <see cref="ISqlDequeueOperation">dequeue operation</see>.</returns>
        public virtual async Task<ISqlDequeueOperation> Dequeue( DbConnection connection, Guid subscriptionId, DateTimeOffset dueTime, CancellationToken cancellationToken )
        {
            Arg.NotNull( connection, nameof( connection ) );
            Contract.Ensures( Contract.Result<Task<ISqlDequeueOperation>>() != null );

            using ( var command = connection.CreateCommand() )
            {
                var parameter = command.CreateParameter();

                parameter.DbType = DbType.Guid;
                parameter.ParameterName = "SubscriptionId";
                parameter.Value = subscriptionId;
                command.Parameters.Add( parameter );

                parameter = command.CreateParameter();
                parameter.DbType = DbType.DateTime2;
                parameter.ParameterName = "DueTime";
                parameter.Value = dueTime.UtcDateTime;
                command.Parameters.Add( parameter );

                command.CommandText = Sql.SubscriptionQueue.Dequeue;

                var transaction = connection.BeginTransaction();

                command.Transaction = transaction;

                using ( var reader = await command.ExecuteReaderAsync( SingleRow, cancellationToken ).ConfigureAwait( false ) )
                {
                    if ( await reader.ReadAsync( cancellationToken ).ConfigureAwait( false ) )
                    {
                        var item = new SqlMessageQueueItem()
                        {
                            SubscriptionId = subscriptionId,
                            MessageId = reader.GetGuid( 0 ),
                            EnqueueTime = reader.GetDateTime( 1 ),
                            DequeueAttempts = reader.GetInt32( 2 ),
                            DueTime = reader.GetDateTime( 3 ),
                            MessageType = reader.GetString( 4 ),
                            Revision = reader.GetInt32( 5 ),
                            Message = reader.GetStream( 6 ),
                            Transaction = transaction,
                        };

                        return new SqlDequeueOperation( item, transaction );
                    }
                }

                transaction.Commit();
                return SqlDequeueOperation.Empty;
            }
        }
    }
}