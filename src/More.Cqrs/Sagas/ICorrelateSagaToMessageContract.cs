// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;

    [ContractClassFor( typeof( ICorrelateSagaToMessage ) )]
    abstract class ICorrelateSagaToMessageContract : ICorrelateSagaToMessage
    {
        void ICorrelateSagaToMessage.Configure<TData, TMessage>( Expression<Func<TData, object>> sagaDataProperty, Expression<Func<TMessage, object>> messageProperty )
        {
            Contract.Requires<ArgumentNullException>( sagaDataProperty != null, nameof( sagaDataProperty ) );
            Contract.Requires<ArgumentNullException>( messageProperty != null, nameof( messageProperty ) );
        }
    }
}