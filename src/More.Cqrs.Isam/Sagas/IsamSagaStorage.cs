// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using Microsoft.Database.Isam;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    /// <summary>
    /// Represents a saga store that is backed by an ISAM database.
    /// </summary>
    public class IsamSagaStorage : IStoreSagaData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsamSagaStorage"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IsamSagaStorageConfiguration">configuration</see> used by the saga store.</param>
        public IsamSagaStorage( IsamSagaStorageConfiguration configuration )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Configuration = configuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IsamSagaStorage"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string used by the saga store.</param>
        public IsamSagaStorage( string connectionString )
        {
            Arg.NotNullOrEmpty( connectionString, nameof( connectionString ) );

            var builder = new IsamSagaStorageConfigurationBuilder().HasConnectionString( connectionString );
            Configuration = builder.CreateConfiguration();
        }

        /// <summary>
        /// Gets the current SQL saga storage configuration.
        /// </summary>
        /// <value>The current <see cref="IsamSagaStorageConfiguration">SQL saga storage configuration</see>.</value>
        protected IsamSagaStorageConfiguration Configuration { get; }

        /// <summary>
        /// Performs the storage completion operations for the saga.
        /// </summary>
        /// <param name="data">The data of the saga to complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public Task Complete( ISagaData data, CancellationToken cancellationToken )
        {
            Arg.NotNull( data, nameof( data ) );

            var connection = Configuration.CreateConnection();

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                using ( var cursor = database.OpenCursor( Configuration.TableName ) )
                using ( var transaction = new IsamTransaction( session ) )
                {
                    Configuration.Complete( cursor, data );
                    transaction.Commit();
                }
            }

            return CompletedTask;
        }

        /// <summary>
        /// Retrieves the data for a saga using the specified identifier.
        /// </summary>
        /// <typeparam name="TData">The type of saga data.</typeparam>
        /// <param name="sagaId">The identifier of the saga to retrieve.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public Task<TData> Retrieve<TData>( Guid sagaId, CancellationToken cancellationToken ) where TData : class, ISagaData
        {
            var connection = Configuration.CreateConnection();
            var data = default( TData );

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                using ( var cursor = database.OpenCursor( Configuration.TableName ) )
                {
                    data = Configuration.Load<TData>( cursor, sagaId );
                }
            }

            return FromResult( data );
        }

        /// <summary>
        /// Retrieves the data for a saga using the specified property name and value.
        /// </summary>
        /// <typeparam name="TData">The type of saga data.</typeparam>
        /// <param name="propertyName">The name of the property to retrieve the saga by.</param>
        /// <param name="propertyValue">The value of the property to retrieve the saga by.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public Task<TData> Retrieve<TData>( string propertyName, object propertyValue, CancellationToken cancellationToken ) where TData : class, ISagaData
        {
            Arg.NotNullOrEmpty( propertyName, nameof( propertyName ) );
            Arg.NotNull( propertyValue, nameof( propertyValue ) );

            var connection = Configuration.CreateConnection();
            var data = default( TData );

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                using ( var cursor = database.OpenCursor( Configuration.TableName ) )
                {
                    data = Configuration.Load<TData>( cursor, propertyName, propertyValue );
                }
            }

            return FromResult( data );
        }

        /// <summary>
        /// Stores the specified saga data.
        /// </summary>
        /// <param name="data">The data to store.</param>
        /// <param name="correlationProperty">The property used to correlate the stored data.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public Task Store( ISagaData data, CorrelationProperty correlationProperty, CancellationToken cancellationToken )
        {
            Arg.NotNull( data, nameof( data ) );
            Arg.NotNull( correlationProperty, nameof( correlationProperty ) );

            Configuration.Store( data, correlationProperty );
            return CompletedTask;
        }
    }
}