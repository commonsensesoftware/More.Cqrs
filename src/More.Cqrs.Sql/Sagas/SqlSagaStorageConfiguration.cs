// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities

namespace More.Domain.Sagas
{
    using More.Domain.Messaging;
    using System;
    using System.Collections.Concurrent;
    using System.Data;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.BitConverter;
    using static System.Linq.Expressions.Expression;
    using static System.Text.Encoding;

    /// <summary>
    /// Represents the configuration for a SQL database saga store.
    /// </summary>
    public class SqlSagaStorageConfiguration
    {
        static readonly MethodInfo SerializeMethodOfT = typeof( SqlSagaStorageConfiguration ).GetRuntimeMethods().Single( m => !m.IsPublic && m.Name == nameof( Serialize ) );
        readonly string connectionString;
        readonly ISqlMessageSerializerFactory serializerFactory;
        readonly ConcurrentDictionary<Type, Func<ISagaData, Stream>> serializers = new ConcurrentDictionary<Type, Func<ISagaData, Stream>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlSagaStorageConfiguration"/> class.
        /// </summary>
        /// <param name="providerFactory">The <see cref="DbProviderFactory">provider factory</see> for underlying database operations.</param>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="tableName">The saga data table name.</param>
        /// <param name="messageTypeResolver">The <see cref="IMessageTypeResolver">resolver</see> used to resolve message <see cref="Type">types</see>.</param>
        /// <param name="serializerFactory">The <see cref="ISqlMessageSerializerFactory">factory</see> used to create new message serializers.</param>
        public SqlSagaStorageConfiguration(
            DbProviderFactory providerFactory,
            string connectionString,
            SqlIdentifier tableName,
            IMessageTypeResolver messageTypeResolver,
            ISqlMessageSerializerFactory serializerFactory )
        {
            ProviderFactory = providerFactory;
            this.connectionString = connectionString;
            this.serializerFactory = serializerFactory;
            TableName = tableName;
            MessageTypeResolver = messageTypeResolver;
            Sql = new SqlBuilder( tableName );
        }

        SqlBuilder Sql { get; }

        /// <summary>
        /// Gets the database provider factory used by the configuration.
        /// </summary>
        /// <value>The configured <see cref="DbProviderFactory">database provider factory</see>.</value>
        protected DbProviderFactory ProviderFactory { get; }

        /// <summary>
        /// Gets the identifier of the saga store table.
        /// </summary>
        /// <vaule>The <see cref="SqlIdentifier">identifier</see> of the saga store table.</vaule>
        public SqlIdentifier TableName { get; }

        /// <summary>
        /// Gets the object used to resolve message types.
        /// </summary>
        /// <value>The <see cref="IMessageTypeResolver">resolver</see> used to resolve message <see cref="Type">types</see>.</value>
        public IMessageTypeResolver MessageTypeResolver { get; }

        /// <summary>
        /// Creates a new message serializer.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to create a serializer for.</typeparam>
        /// <returns>A new <see cref="ISqlMessageSerializer{TMessage}"/>.</returns>
        public ISqlMessageSerializer<TMessage> NewMessageSerializer<TMessage>() where TMessage : class => serializerFactory.NewSerializer<TMessage>();

        /// <summary>
        /// Create a new database connection.
        /// </summary>
        /// <returns>A new, configured <see cref="DbConnection">database connection</see>.</returns>
        public virtual DbConnection CreateConnection()
        {
            var connection = ProviderFactory.CreateConnection();
            connection.ConnectionString = connectionString;
            return connection;
        }

        /// <summary>
        /// Creates the saga storage tables.
        /// </summary>
        /// <param name="cancellationToken">The optional <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual async Task CreateTables( CancellationToken cancellationToken = default )
        {
            using var connection = CreateConnection();

            await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );

            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();

            command.Transaction = transaction;
            command.CommandText = Sql.CreateSchema;
            await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );

            command.CommandText = Sql.CreateTable;
            await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );

            command.CommandText = Sql.CreateIndex;
            await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );

            transaction.Commit();
        }

        /// <summary>
        /// Creates a command to query a saga by its unique identifier.
        /// </summary>
        /// <param name="sagaId">The identifier of the saga to create the command for.</param>
        /// <returns>A new, configured <see cref="DbCommand">command</see>.</returns>
        public virtual DbCommand NewQueryByIdCommand( Guid sagaId )
        {
            var command = ProviderFactory.CreateCommand();
            var parameter = command.CreateParameter();

            parameter.DbType = DbType.Guid;
            parameter.ParameterName = "SagaId";
            parameter.Value = sagaId;
            command.Parameters.Add( parameter );
            command.CommandText = Sql.QueryById;

            return command;
        }

        /// <summary>
        /// Creates a command to query a saga by a correlated property value.
        /// </summary>
        /// <param name="dataType">The qualified name of the saga data type to build the command for.</param>
        /// <param name="propertyName">The saga property to query.</param>
        /// <param name="propertyValue">The saga property value to match.</param>
        /// <returns>A new, configured <see cref="DbCommand">command</see>.</returns>
        public virtual DbCommand NewQueryByPropertyCommand( string dataType, string propertyName, object propertyValue )
        {
            var command = ProviderFactory.CreateCommand();
            var parameter = command.CreateParameter();

            parameter.DbType = DbType.String;
            parameter.Size = 256;
            parameter.ParameterName = "DataType";
            parameter.Value = dataType;
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.DbType = DbType.String;
            parameter.Size = 90;
            parameter.ParameterName = "PropertyName";
            parameter.Value = propertyName;
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.DbType = DbType.Binary;
            parameter.Size = 200;
            parameter.ParameterName = "PropertyValue";
            parameter.Value = ValueAsBinary( propertyValue );
            command.Parameters.Add( parameter );

            command.CommandText = Sql.QueryByProperty;

            return command;
        }

        /// <summary>
        /// Creates a command to store a saga.
        /// </summary>
        /// <param name="saga">The <see cref="ISagaData">saga data</see> to create the command for.</param>
        /// <param name="correlationProperty">The property used to correlate the stored data.</param>
        /// <param name="data">The serialized saga data.</param>
        /// <returns>A new, configured <see cref="DbCommand">command</see>.</returns>
        public virtual DbCommand NewStoreCommand( ISagaData saga, CorrelationProperty correlationProperty, Stream data )
        {
            var command = ProviderFactory.CreateCommand();
            var parameter = command.CreateParameter();

            parameter.DbType = DbType.Guid;
            parameter.ParameterName = "SagaId";
            parameter.Value = saga.Id;
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.DbType = DbType.String;
            parameter.Size = 256;
            parameter.ParameterName = "DataType";
            parameter.Value = saga.GetType().GetAssemblyQualifiedName();
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.DbType = DbType.String;
            parameter.Size = 90;
            parameter.ParameterName = "PropertyName";
            parameter.Value = correlationProperty.Property.Name;
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.DbType = DbType.Binary;
            parameter.Size = 200;
            parameter.ParameterName = "PropertyValue";
            parameter.Value = ValueAsBinary( correlationProperty.Value );
            command.Parameters.Add( parameter );

            parameter = command.CreateParameter();
            parameter.DbType = DbType.Binary;
            parameter.ParameterName = "Data";
            parameter.Value = data;
            command.Parameters.Add( parameter );

            command.CommandText = Sql.Store;

            return command;
        }

        /// <summary>
        /// Creates a command to store a saga.
        /// </summary>
        /// <param name="saga">The <see cref="ISagaData">saga data</see> to create the command for.</param>
        /// <param name="correlationProperty">The property used to correlate the stored data.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A new, configured <see cref="DbCommand">command</see>.</returns>
        public virtual async Task Store( ISagaData saga, CorrelationProperty correlationProperty, CancellationToken cancellationToken )
        {
            using var connection = CreateConnection();
            await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );
            await Store( connection, saga, correlationProperty, cancellationToken ).ConfigureAwait( false );
        }

        /// <summary>
        /// Creates a command to store a saga.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection">connection</see> to create the command from.</param>
        /// <param name="saga">The <see cref="ISagaData">saga data</see> to create the command for.</param>
        /// <param name="correlationProperty">The property used to correlate the stored data.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A new, configured <see cref="DbCommand">command</see>.</returns>
        public virtual async Task Store( DbConnection connection, ISagaData saga, CorrelationProperty correlationProperty, CancellationToken cancellationToken )
        {
            using var stream = Serialize( saga );
            using var command = NewStoreCommand( saga, correlationProperty, stream );
            command.Connection = connection;
            await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );
        }

        /// <summary>
        /// Creates a command to complete a saga.
        /// </summary>
        /// <param name="saga">The <see cref="ISagaData">saga data</see> to create the command for.</param>
        /// <returns>A new, configured <see cref="DbCommand">command</see>.</returns>
        public virtual DbCommand NewCompleteCommand( ISagaData saga )
        {
            var command = ProviderFactory.CreateCommand();
            var parameter = command.CreateParameter();

            parameter.ParameterName = "SagaId";
            parameter.DbType = DbType.Guid;
            parameter.Value = saga.Id;

            command.Parameters.Add( parameter );
            command.CommandText = Sql.Completed;

            return command;
        }

        /// <summary>
        /// Serializes the specified saga data.
        /// </summary>
        /// <param name="data">The <see cref="ISagaData">data</see> to serialize.</param>
        /// <returns>A <see cref="Stream">stream</see> containing the serialized saga data.</returns>
        public virtual Stream Serialize( ISagaData data ) => serializers.GetOrAdd( data.GetType(), NewSerializer )( data );

        /// <summary>
        /// Converts the specified value to binary.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value in binary form.</returns>
        protected virtual byte[] ValueAsBinary( object value ) =>
            value switch
            {
                short @short => GetBytes( @short ),
                int @int => GetBytes( @int ),
                long @long => GetBytes( @long ),
                ushort @ushort => GetBytes( @ushort ),
                uint @uint => GetBytes( @uint ),
                ulong @ulong => GetBytes( @ulong ),
                Guid guid => guid.ToByteArray(),
                string @string => Unicode.GetBytes( @string ),
                _ => throw new ArgumentException( SR.UnsupportedCorrelationValueType.FormatDefault( value.GetType().Name ) ),
            };

        Func<ISagaData, Stream> NewSerializer( Type dataType )
        {
            var data = Parameter( typeof( ISagaData ), "data" );
            var method = SerializeMethodOfT.MakeGenericMethod( dataType );
            var body = Call( Constant( this ), method, Convert( data, dataType ) );
            var lambda = Lambda<Func<ISagaData, Stream>>( body, data );

            return lambda.Compile();
        }

        Stream Serialize<TData>( TData data ) where TData : class, ISagaData => NewMessageSerializer<TData>().Serialize( data );
    }
}