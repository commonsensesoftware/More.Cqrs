// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior of a repository of aggregates.
    /// </summary>
    /// <typeparam name="TKey">The type of key defined by the aggregate.</typeparam>
    /// <typeparam name="TAggregate">The type of aggregates in the repository.</typeparam>
    [ContractClass( typeof( IRepositoryContract<,> ) )]
    public interface IRepository<TKey, TAggregate> where TAggregate : class, IAggregate<TKey>
    {
        /// <summary>
        /// Returns a single aggregate from the repository.
        /// </summary>
        /// <param name="id">The unique aggregate identifier.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">cancellation token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <typeparamref name="TAggregate">aggregate</typeparamref> retrieved asynchronously.</returns>
        Task<TAggregate> Single( TKey id, CancellationToken cancellationToken );

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