// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Persistence
{
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior of persistence.
    /// </summary>
    [ContractClass( typeof( IPersistenceContract ) )]
    public interface IPersistence
    {
        /// <summary>
        /// Persists the specified commit.
        /// </summary>
        /// <param name="commit">The <see cref="Commit">commit</see> to append.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        Task Persist( Commit commit, CancellationToken cancellationToken );
    }
}