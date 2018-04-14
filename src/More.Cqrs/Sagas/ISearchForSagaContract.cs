// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    [ContractClassFor( typeof( ISearchForSaga ) )]
    abstract class ISearchForSagaContract : ISearchForSaga
    {
        Task<SagaSearchResult> ISearchForSaga.Search( SagaSearchMethod searchMethod, object message, CancellationToken cancellationToken )
        {
            Contract.Requires<ArgumentNullException>( searchMethod != null, nameof( searchMethod ) );
            Contract.Requires<ArgumentNullException>( message != null, nameof( message ) );
            Contract.Ensures( Contract.Result<Task<SagaSearchResult>>() != null );
            return null;
        }
    }
}