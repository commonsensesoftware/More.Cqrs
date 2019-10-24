// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

#pragma warning disable CA1716 // Identifiers should not match keywords
#pragma warning disable CA1720 // Identifier contains type name

namespace More.Domain
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides extension methods for the <see cref="IRepository{TKey, TAggregate}"/> interface.
    /// </summary>
    public static class IRepositoryExtensions
    {
        /// <summary>
        /// Returns a single aggregate from the repository.
        /// </summary>
        /// <typeparam name="TKey">The type of key defined by the aggregate.</typeparam>
        /// <typeparam name="TAggregate">The type of aggregates in the repository.</typeparam>
        /// <param name="repository">The extended <see cref="IRepository{TKey, TAggregate}">repository</see>.</param>
        /// <param name="id">The unique aggregate identifier.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">cancellation token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <typeparamref name="TAggregate">aggregate</typeparamref> retrieved asynchronously.</returns>
        public static Task<TAggregate> Single<TKey, TAggregate>( this IRepository<TKey, TAggregate> repository, TKey id, CancellationToken cancellationToken = default )
            where TKey : notnull
            where TAggregate : class, IAggregate<TKey> => repository.Single( id, default, cancellationToken );
    }
}