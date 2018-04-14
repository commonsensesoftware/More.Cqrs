// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Data.CommandBehavior;

    /// <summary>
    /// Represents a saga store that is backed by a SQL database.
    /// </summary>
    public class SqlSagaStorage : IStoreSagaData
    {
        const int Revision = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlSagaStorage"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="SqlSagaStorageConfiguration">configuration</see> used by the saga store.</param>
        public SqlSagaStorage( SqlSagaStorageConfiguration configuration )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Configuration = configuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlSagaStorage"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string used by the saga store.</param>
        public SqlSagaStorage( string connectionString )
        {
            Arg.NotNullOrEmpty( connectionString, nameof( connectionString ) );

            var builder = new SqlSagaStorageConfigurationBuilder().HasConnectionString( connectionString );
            Configuration = builder.CreateConfiguration();
        }

        /// <summary>
        /// Gets the current SQL saga storage configuration.
        /// </summary>
        /// <value>The current <see cref="SqlSagaStorageConfiguration">SQL saga storage configuration</see>.</value>
        protected SqlSagaStorageConfiguration Configuration { get; }

        /// <summary>
        /// Performs the storage completion operations for the saga.
        /// </summary>
        /// <param name="data">The data of the saga to complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> repesenting the asynchronous operation.</returns>
        public async Task Complete( ISagaData data, CancellationToken cancellationToken )
        {
            Arg.NotNull( data, nameof( data ) );

            using ( var connection = Configuration.CreateConnection() )
            {
                await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );

                using ( var command = Configuration.NewCompleteCommand( data ) )
                {
                    command.Connection = connection;
                    await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );
                }
            }
        }

        /// <summary>
        /// Retrieves the data for a saga using the specified identifier.
        /// </summary>
        /// <typeparam name="TData">The type of saga data.</typeparam>
        /// <param name="sagaId">The identifier of the saga to retrieve.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> repesenting the asynchronous operation.</returns>
        public async Task<TData> Retrieve<TData>( Guid sagaId, CancellationToken cancellationToken ) where TData : class, ISagaData
        {
            using ( var connection = Configuration.CreateConnection() )
            {
                await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );

                using ( var query = Configuration.NewQueryByIdCommand( sagaId ) )
                {
                    query.Connection = connection;

                    using ( var reader = await query.ExecuteReaderAsync( SingleRow | SequentialAccess, cancellationToken ).ConfigureAwait( false ) )
                    {
                        if ( await reader.ReadAsync( cancellationToken ).ConfigureAwait( false ) )
                        {
                            var messageTypeName = reader.GetString( 0 );
                            var actualMessageType = Configuration.MessageTypeResolver.ResolveType( messageTypeName, Revision );
                            var requestedMessageType = typeof( TData );

                            if ( actualMessageType != requestedMessageType )
                            {
                                return default( TData );
                            }

                            using ( var stream = reader.GetStream( 1 ) )
                            {
                                return Configuration.NewMessageSerializer<TData>().Deserialize( messageTypeName, Revision, stream );
                            }
                        }

                        return default( TData );
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the data for a saga using the specified property name and value.
        /// </summary>
        /// <typeparam name="TData">The type of saga data.</typeparam>
        /// <param name="propertyName">The name of the property to retrieve the saga by.</param>
        /// <param name="propertyValue">The value of the property to retrieve the saga by.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> repesenting the asynchronous operation.</returns>
        public async Task<TData> Retrieve<TData>( string propertyName, object propertyValue, CancellationToken cancellationToken ) where TData : class, ISagaData
        {
            Arg.NotNullOrEmpty( propertyName, nameof( propertyName ) );
            Arg.NotNull( propertyValue, nameof( propertyValue ) );

            var messageType = typeof( TData ).GetAssemblyQualifiedName();

            using ( var connection = Configuration.CreateConnection() )
            {
                await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );

                using ( var query = Configuration.NewQueryByPropertyCommand( messageType, propertyName, propertyValue ) )
                {
                    query.Connection = connection;

                    using ( var reader = await query.ExecuteReaderAsync( SingleRow | SequentialAccess, cancellationToken ).ConfigureAwait( false ) )
                    {
                        if ( await reader.ReadAsync( cancellationToken ).ConfigureAwait( false ) )
                        {
                            using ( var stream = reader.GetStream( 0 ) )
                            {
                                return Configuration.NewMessageSerializer<TData>().Deserialize( messageType, Revision, stream );
                            }
                        }

                        return default( TData );
                    }
                }
            }
        }

        /// <summary>
        /// Stores the specified saga data.
        /// </summary>
        /// <param name="data">The data to store.</param>
        /// <param name="correlationProperty">The property used to correlate the stored data.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> repesenting the asynchronous operation.</returns>
        public Task Store( ISagaData data, CorrelationProperty correlationProperty, CancellationToken cancellationToken )
        {
            Arg.NotNull( data, nameof( data ) );
            Arg.NotNull( correlationProperty, nameof( correlationProperty ) );
            return Configuration.Store( data, correlationProperty, cancellationToken );
        }
    }
}