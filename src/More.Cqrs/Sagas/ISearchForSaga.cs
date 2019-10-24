// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior of an object that searches for saga data.
    /// </summary>
    public interface ISearchForSaga
    {
        /// <summary>
        /// Searches for saga data using the specified search method and message.
        /// </summary>
        /// <param name="searchMethod">The <see cref="SagaSearchMethod">search method</see> used to find the saga data.</param>
        /// <param name="message">The correlated message used to find the saga data.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <see cref="SagaSearchResult">search result</see>.</returns>
        Task<SagaSearchResult> Search( SagaSearchMethod searchMethod, object message, CancellationToken cancellationToken );
    }
}