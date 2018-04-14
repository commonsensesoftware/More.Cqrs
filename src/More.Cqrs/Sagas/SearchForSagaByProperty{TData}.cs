// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an object that searches for saga data using the property of a message.
    /// </summary>
    /// <typeparam name="TData">The type of saga data.</typeparam>
    public sealed class SearchForSagaByProperty<TData> : ISearchForSaga where TData : class, ISagaData
    {
        readonly IStoreSagaData store;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchForSagaByProperty{TData}"/> class.
        /// </summary>
        /// <param name="sagaStorage">The underlying <see cref="IStoreSagaData">saga storage</see>.</param>
        public SearchForSagaByProperty( IStoreSagaData sagaStorage )
        {
            Arg.NotNull( sagaStorage, nameof( sagaStorage ) );
            store = sagaStorage;
        }

        /// <summary>
        /// Seaches for saga data using the specified search method and message.
        /// </summary>
        /// <param name="searchMethod">The <see cref="SagaSearchMethod">search method</see> used to find the saga data.</param>
        /// <param name="messsage">The correlated message used to find the saga data.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <see cref="SagaSearchResult">search result</see>.</returns>
        public async Task<SagaSearchResult> Search( SagaSearchMethod searchMethod, object messsage, CancellationToken cancellationToken )
        {
            Arg.NotNull( searchMethod, nameof( searchMethod ) );
            Arg.NotNull( messsage, nameof( messsage ) );

            if ( !( searchMethod is ByPropertySagaSearchMethod propertySearchMethod ) )
            {
                return new SagaSearchResult();
            }

            var messagePropertyValue = propertySearchMethod.ReadMessageProperty( messsage );
            var sagaPropertyName = propertySearchMethod.PropertyName;
            var properties = new Dictionary<string, object>() { [sagaPropertyName] = messagePropertyValue };
            var data = default( TData );

            if ( messagePropertyValue == null )
            {
                return new SagaSearchResult( data, properties );
            }

            if ( StringComparer.OrdinalIgnoreCase.Equals( sagaPropertyName, nameof( ISagaData.Id ) ) )
            {
                data = await store.Retrieve<TData>( (Guid) messagePropertyValue, cancellationToken ).ConfigureAwait( false );
            }
            else
            {
                data = await store.Retrieve<TData>( sagaPropertyName, messagePropertyValue, cancellationToken ).ConfigureAwait( false );
            }

            return new SagaSearchResult( data, properties );
        }
    }
}