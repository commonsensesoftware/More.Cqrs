// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    [ContractClassFor( typeof( IStoreSagaData ) )]
    abstract class IStoreSagaDataContract : IStoreSagaData
    {
        Task IStoreSagaData.Store( ISagaData data, CorrelationProperty correlationProperty, CancellationToken cancellationToken )
        {
            Contract.Requires<ArgumentNullException>( data != null, nameof( data ) );
            Contract.Requires<ArgumentNullException>( correlationProperty != null, nameof( correlationProperty ) );
            Contract.Ensures( Contract.Result<Task>() != null );
            return null;
        }

        Task<TData> IStoreSagaData.Retrieve<TData>( Guid sagaId, CancellationToken cancellationToken )
        {
            Contract.Ensures( Contract.Result<Task<TData>>() != null );
            return null;
        }

        Task<TData> IStoreSagaData.Retrieve<TData>( string propertyName, object propertyValue, CancellationToken cancellationToken )
        {
            Contract.Requires<ArgumentNullException>( !string.IsNullOrEmpty( propertyName ), nameof( propertyName ) );
            Contract.Ensures( Contract.Result<Task<TData>>() != null );
            return null;
        }

        Task IStoreSagaData.Complete( ISagaData data, CancellationToken cancellationToken )
        {
            Contract.Requires<ArgumentNullException>( data != null, nameof( data ) );
            Contract.Ensures( Contract.Result<Task>() != null );
            return null;
        }
    }
}