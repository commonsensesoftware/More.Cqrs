// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Options
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using static System.Linq.Enumerable;

    sealed class DefaultOptions : IOptions
    {
        DefaultOptions() { }

        internal static IOptions None { get; } = new DefaultOptions();

        public T Get<T>() where T : notnull => throw new KeyNotFoundException( SR.KeyNotFound.FormatDefault( typeof( T ) ) );

        public bool TryGet<T>( [NotNullWhen( true )] out T option ) where T : class
        {
            option = default!;
            return false;
        }

        public IEnumerable<T> All<T>() where T : notnull => Empty<T>();
    }
}