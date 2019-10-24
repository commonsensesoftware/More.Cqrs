// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;
    using System.Collections.Generic;

    sealed class KeyedCollection<TKey, TItem> : System.Collections.ObjectModel.KeyedCollection<TKey, TItem>
        where TKey : notnull
        where TItem : class
    {
        readonly Func<TItem, TKey> keySelector;

        internal KeyedCollection( Func<TItem, TKey> keySelector ) => this.keySelector = keySelector;

        internal KeyedCollection( Func<TItem, TKey> keySelector, IEqualityComparer<TKey> comparer )
            : base( comparer ) => this.keySelector = keySelector;

        protected override TKey GetKeyForItem( TItem item ) => keySelector( item );
    }
}