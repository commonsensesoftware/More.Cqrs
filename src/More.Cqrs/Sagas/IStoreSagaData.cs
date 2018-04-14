// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior of an object that stores saga data.
    /// </summary>
    [ContractClass( typeof( IStoreSagaDataContract ) )]
    public interface IStoreSagaData
    {
        /// <summary>
        /// Stores the specified saga data.
        /// </summary>
        /// <param name="data">The data to store.</param>
        /// <param name="correlationProperty">The property used to correlate the stored data.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> repesenting the asynchronous operation.</returns>
        Task Store( ISagaData data, CorrelationProperty correlationProperty, CancellationToken cancellationToken );

        /// <summary>
        /// Retrieves the data for a saga using the specified identifier.
        /// </summary>
        /// <typeparam name="TData">The type of saga data.</typeparam>
        /// <param name="sagaId">The identifier of the saga to retrieve.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> repesenting the asynchronous operation.</returns>
        Task<TData> Retrieve<TData>( Guid sagaId, CancellationToken cancellationToken ) where TData : class, ISagaData;

        /// <summary>
        /// Retrieves the data for a saga using the specified property name and value.
        /// </summary>
        /// <typeparam name="TData">The type of saga data.</typeparam>
        /// <param name="propertyName">The name of the property to retrieve the saga by.</param>
        /// <param name="propertyValue">The value of the property to retrieve the saga by.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> repesenting the asynchronous operation.</returns>
        Task<TData> Retrieve<TData>( string propertyName, object propertyValue, CancellationToken cancellationToken ) where TData : class, ISagaData;

        /// <summary>
        /// Performs the storage completion operations for the saga.
        /// </summary>
        /// <param name="data">The completed saga data.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> repesenting the asynchronous operation.</returns>
        Task Complete( ISagaData data, CancellationToken cancellationToken );
    }
}