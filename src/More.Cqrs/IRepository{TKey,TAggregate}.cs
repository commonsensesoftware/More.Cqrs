// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

#pragma warning disable CA1716 // Identifiers should not match keywords
#pragma warning disable CA1720 // Identifier contains type name

namespace More.Domain
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior of a repository of aggregates.
    /// </summary>
    /// <typeparam name="TKey">The type of key defined by the aggregate.</typeparam>
    /// <typeparam name="TAggregate">The type of aggregates in the repository.</typeparam>
    public interface IRepository<TKey, TAggregate>
        where TKey : notnull
        where TAggregate : class, IAggregate<TKey>
    {
        /// <summary>
        /// Returns a single aggregate from the repository.
        /// </summary>
        /// <param name="id">The unique aggregate identifier.</param>
        /// <param name="predicate">The optional <see cref="IEventPredicate{TKey}">predicate</see> used to retrieve the aggregate.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">cancellation token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <typeparamref name="TAggregate">aggregate</typeparamref> retrieved asynchronously.</returns>
        Task<TAggregate> Single( TKey id, IEventPredicate<TKey>? predicate, CancellationToken cancellationToken = default );

        /// <summary>
        /// Saves the specified aggregate.
        /// </summary>
        /// <param name="aggregate">The <typeparamref name="TAggregate">aggregate</typeparamref> to save.</param>
        /// <param name="expectedVersion">The expected version of the saved aggregate.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">cancellation token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        Task Save( TAggregate aggregate, int expectedVersion, CancellationToken cancellationToken );
    }
}