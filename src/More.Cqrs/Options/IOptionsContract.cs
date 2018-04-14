// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Options
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    [ContractClassFor( typeof( IOptions ) )]
    abstract class IOptionsContract : IOptions
    {
        IEnumerable<T> IOptions.All<T>()
        {
            Contract.Ensures( Contract.Result<IEnumerable<T>>() != null );
            return null;
        }

        T IOptions.Get<T>()
        {
            Contract.Ensures( Contract.Result<T>() != null );
            return default( T );
        }

        bool IOptions.TryGet<T>( out T option )
        {
            Contract.Ensures( Contract.Result<bool>() && Contract.ValueAtReturn( out option ) != null );
            Contract.Ensures( !Contract.Result<bool>() && Contract.ValueAtReturn( out option ) == null );
            option = default( T );
            return default( bool );
        }
    }
}