namespace Contoso.Domain
{
    using System;
    using System.Collections.Generic;

    public class KeyedCollection<TKey, TItem> : System.Collections.ObjectModel.KeyedCollection<TKey, TItem>
    {
        readonly Func<TItem, TKey> keySelector;

        public KeyedCollection( Func<TItem, TKey> keySelector ) => this.keySelector = keySelector;

        public KeyedCollection( Func<TItem, TKey> keySelector, IEqualityComparer<TKey> comparer )
            : base( comparer ) => this.keySelector = keySelector;

        protected override TKey GetKeyForItem( TItem item ) => keySelector( item );

        public void AddOrUpdate( TItem item )
        {
            if ( Count == 0 )
            {
                Add( item );
                return;
            }

            var key = GetKeyForItem( item );

            if ( Contains( key ) )
            {
                for ( var i = 0; i < Count; i++ )
                {
                    if ( Comparer.Equals( key, GetKeyForItem( this[i] ) ) )
                    {
                        this[i] = item;
                        return;
                    }
                }
            }
            else
            {
                Add( item );
            }
        }
    }
}