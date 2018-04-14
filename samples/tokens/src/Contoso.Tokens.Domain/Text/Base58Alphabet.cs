namespace Contoso.Text
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    public class Base58Alphabet
    {
        readonly string alphabet;
        readonly Lazy<Dictionary<char, int>> lookup;

        protected Base58Alphabet( string alphabet )
        {
            Contract.Requires( !string.IsNullOrEmpty( alphabet ) );
            Contract.Requires( alphabet.Length == 58 );

            this.alphabet = alphabet;
            LeadingZero = alphabet[0];
            lookup = new Lazy<Dictionary<char, int>>( CreateLookup );
        }

        Dictionary<char, int> CreateLookup()
        {
            var lookup = new Dictionary<char, int>();

            for ( var i = 0; i < 58; i++ )
            {
                lookup[alphabet[i]] = i;
            }

            return lookup;
        }

        public char LeadingZero { get; }

        public char GetDigit( int index )
        {
            Contract.Requires( index >= 0 && index < 58 );
            return alphabet[index];
        }

        public int GetOffset( char digit )
        {
            Contract.Ensures( Contract.Result<int>() >= 0 && Contract.Result<int>() < 58 );

            if ( lookup.Value.TryGetValue( digit, out var offset ) )
            {
                return offset;
            }

            throw new FormatException( $"The character '{digit}' is not valid for the current Base58 alphabet." );
        }
    }
}