// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Persistence
{
    using System;
    using System.Diagnostics.Contracts;

    [ContractClassFor( typeof( IMapPersistence ) )]
    abstract class IMapPersistenceContract : IMapPersistence
    {
        void IMapPersistence.Add( string entityName, IPersistence persistence )
        {
            Contract.Requires<ArgumentNullException>( !string.IsNullOrEmpty( entityName ), nameof( entityName ) );
            Contract.Requires<ArgumentNullException>( persistence != null );
        }

        IPersistence IMapPersistence.Map( string entityName )
        {
            Contract.Requires<ArgumentNullException>( !string.IsNullOrEmpty( entityName ), nameof( entityName ) );
            Contract.Ensures( Contract.Result<IPersistence>() != null );
            return null;
        }
    }
}