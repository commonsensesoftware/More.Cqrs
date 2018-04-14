namespace Contoso.Text
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Numerics;
    using System.Text;
    using static System.Byte;

    public class Base58Decoder : Decoder
    {
        const int Shift = 256;
        readonly StringBuilder buffer = new StringBuilder();

        public Base58Decoder( Base58Alphabet alphabet )
        {
            Contract.Requires( alphabet != null );
            Alphabet = alphabet;
        }

        protected Base58Alphabet Alphabet { get; }

        public override int GetCharCount( byte[] bytes, int index, int count )
        {
            if ( count == 0 )
            {
                return 0;
            }

            buffer.Length = 0;

            var integer = new BigInteger();

            for ( var i = index; i < count; i++ )
            {
                integer = ( integer * Shift ) + bytes[i];
            }

            while ( integer > 0 )
            {
                var remainder = (int) ( integer % 58 );
                integer /= 58;
                buffer.Insert( 0, Alphabet.GetDigit( remainder ) );
            }

            var leadingZero = Alphabet.LeadingZero;

            for ( var i = 0; i < count && bytes[i] == MinValue; i++ )
            {
                buffer.Insert( 0, leadingZero );
            }

            return buffer.Length;
        }

        public override int GetChars( byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex )
        {
            if ( byteCount == 0 )
            {
                return 0;
            }

            var count = buffer.Length;
            buffer.CopyTo( 0, chars, charIndex, count );
            return count;
        }

        public override void Reset()
        {
            base.Reset();
            buffer.Length = 0;
        }
    }
}