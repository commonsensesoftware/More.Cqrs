// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Linq.Expressions.Expression;
    using static System.Threading.Tasks.Task;
    using PredicateCollection = System.Collections.Concurrent.ConcurrentDictionary<string, System.Func<More.Domain.Sagas.ISagaData, object, bool>>;
    using RetrieverCollection = System.Collections.Concurrent.ConcurrentDictionary<System.Type, System.Collections.Concurrent.ConcurrentDictionary<string, System.Func<More.Domain.Sagas.ISagaData, object, bool>>>;

    /// <summary>
    /// Represents an in-memory saga store.
    /// </summary>
    public class InMemorySagaStorage : IStoreSagaData
    {
        static readonly MethodInfo EqualsMethod = typeof( object ).GetRuntimeMethod( nameof( Equals ), new[] { typeof( object ), typeof( object ) } )!;
        readonly ConcurrentDictionary<Guid, ISagaData> storage = new ConcurrentDictionary<Guid, ISagaData>();
        readonly RetrieverCollection retrievers = new RetrieverCollection();
        readonly List<ISagaData> completedSagas = new List<ISagaData>();

        /// <summary>
        /// Stores the specified data.
        /// </summary>
        /// <param name="data">The <see cref="ISagaData">data</see> to store.</param>
        /// <param name="correlationProperty">The property used to correlate the stored data.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual Task Store( ISagaData data, CorrelationProperty? correlationProperty, CancellationToken cancellationToken )
        {
            storage.AddOrUpdate( data.Id, data, ( key, current ) => data );
            return CompletedTask;
        }

        /// <summary>
        /// Retrieves the stored saga data.
        /// </summary>
        /// <typeparam name="TData">The type of data.</typeparam>
        /// <param name="sagaId">The saga identifier.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the retrieved <see cref="ISagaData">saga data</see>.</returns>
        public virtual Task<TData?> Retrieve<TData>( Guid sagaId, CancellationToken cancellationToken ) where TData : class, ISagaData
        {
            var data = default( TData );

            if ( storage.TryGetValue( sagaId, out var storedValue ) )
            {
                data = (TData) storedValue;
            }

            return FromResult( data );
        }

        /// <summary>
        /// Retrieves the stored saga data.
        /// </summary>
        /// <typeparam name="TData">The type of data.</typeparam>
        /// <param name="propertyName">The name of the property to look up the data by.</param>
        /// <param name="propertyValue">The value of the property to look up the data with.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the retrieved <see cref="ISagaData">saga data</see>.</returns>
        public virtual Task<TData?> Retrieve<TData>( string propertyName, object propertyValue, CancellationToken cancellationToken ) where TData : class, ISagaData
        {
            var dataType = typeof( TData );
            var predicates = retrievers.GetOrAdd( dataType, key => new PredicateCollection() );
            var predicate = predicates.GetOrAdd( propertyName, key => NewPredicate( dataType, propertyName ) );
            var data = storage.Values.OfType<TData>().FirstOrDefault( d => predicate( d, propertyValue ) );

            return FromResult<TData?>( data );
        }

        /// <summary>
        /// Stores the completed saga data.
        /// </summary>
        /// <param name="data">The <see cref="ISagaData">data</see> to store.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual Task Complete( ISagaData data, CancellationToken cancellationToken )
        {
            if ( completedSagas.Contains( data ) )
            {
                completedSagas.Add( data );
            }

            return CompletedTask;
        }

        /// <summary>
        /// Gets a read-only list of the completed sagas.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ISagaData">sagas</see>.</value>
        public IReadOnlyList<ISagaData> CompletedSagas => completedSagas;

        static Func<ISagaData, object, bool> NewPredicate( Type dataType, string propertyName )
        {
            var data = Parameter( typeof( ISagaData ), "data" );
            var value = Parameter( typeof( object ), "value" );
            var property = Property( Convert( data, dataType ), propertyName );
            var body = Call( EqualsMethod, Convert( property, typeof( object ) ), value );
            var lambda = Lambda<Func<ISagaData, object, bool>>( body, data, value );

            return lambda.Compile();
        }
    }
}