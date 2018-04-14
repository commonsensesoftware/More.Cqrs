// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;
    using System.Collections.Generic;

    sealed class KeyedCollection<TKey, TItem> : System.Collections.ObjectModel.KeyedCollection<TKey, TItem>
    {
        readonly Func<TItem, TKey> keySelector;

        internal KeyedCollection( Func<TItem, TKey> keySelector ) => this.keySelector = keySelector;

        internal KeyedCollection( Func<TItem, TKey> keySelector, IEqualityComparer<TKey> comparer ) : base( comparer )
        {
            this.keySelector = keySelector;
        }

        protected override TKey GetKeyForItem( TItem item ) => keySelector( item );

        internal bool TryGetValue( TKey key, out TItem item )
        {
            if ( Count == 0 )
            {
                item = default( TItem );
                return false;
            }

            return Dictionary.TryGetValue( key, out item );
        }
    }
}