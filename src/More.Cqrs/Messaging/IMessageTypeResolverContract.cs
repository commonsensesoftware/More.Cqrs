// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;
    using System.Diagnostics.Contracts;

    [ContractClassFor( typeof( IMessageTypeResolver ) )]
    abstract class IMessageTypeResolverContract : IMessageTypeResolver
    {
        Type IMessageTypeResolver.ResolveType( string typeName, int revision )
        {
            Contract.Requires<ArgumentNullException>( !string.IsNullOrEmpty( typeName ), nameof( typeName ) );
            Contract.Ensures( Contract.Result<Type>() != null );
            return null;
        }
    }
}