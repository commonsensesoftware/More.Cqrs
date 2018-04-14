// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    [ContractClassFor( typeof( IRepository<,> ) )]
    abstract class IRepositoryContract<TKey, TAggregate> : IRepository<TKey, TAggregate> where TAggregate : class, IAggregate<TKey>
    {
        Task<TAggregate> IRepository<TKey, TAggregate>.Single( TKey id, CancellationToken cancellationToken )
        {
            Contract.Ensures( Contract.Result<Task<TAggregate>>() != null );
            return null;
        }

        Task IRepository<TKey, TAggregate>.Save( TAggregate aggregate, int expectedVersion, CancellationToken cancellationToken )
        {
            Contract.Requires<ArgumentNullException>( aggregate != null, nameof( aggregate ) );
            Contract.Ensures( Contract.Result<Task>() != null );
            return null;
        }
    }
}