// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using More.Domain.Events;
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;

    [ContractClassFor( typeof( Saga<> ) )]
    abstract class SagaContract<TData> : Saga<TData> where TData : class, ISagaData
    {
        protected override void CorrelateUsing( SagaCorrelator<TData> correlator )
        {
            Contract.Requires<ArgumentNullException>( correlator != null, nameof( correlator ) );
        }
    }
}