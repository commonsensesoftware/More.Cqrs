// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Persistence
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    [ContractClassFor( typeof( IPersistence ) )]
    abstract class IPersistenceContract : IPersistence
    {
        Task IPersistence.Persist( Commit record, CancellationToken cancellationToken )
        {
            Contract.Requires<ArgumentNullException>( record != null );
            Contract.Ensures( Contract.Result<Task>() != null );
            return null;
        }
    }
}