// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System.Diagnostics.Contracts;

    [ContractClassFor( typeof( ISqlMessageSerializerFactory ) )]
    abstract class ISqlMessageSerializerFactoryContract : ISqlMessageSerializerFactory
    {
        ISqlMessageSerializer<TMessage> ISqlMessageSerializerFactory.NewSerializer<TMessage>()
        {
            Contract.Ensures( Contract.Result<ISqlMessageSerializer<TMessage>>() != null );
            return null;
        }
    }
}