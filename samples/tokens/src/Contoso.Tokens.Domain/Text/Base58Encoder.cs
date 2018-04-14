namespace Contoso.Text
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Numerics;
    using System.Text;
    using static System.Array;
    using static System.BitConverter;
    using static System.Byte;

    public class Base58Encoder : Encoder
    {
        byte[] buffer;
        int size;

        public Base58Encoder( Base58Alphabet alphabet )
        {
            Contract.Requires( alphabet != null );
            Alphabet = alphabet;
        }

        protected Base58Alphabet Alphabet { get; }

        public override int GetByteCount( char[] chars, int index, int count, bool flush )
        {
            buffer = new byte[count];
            size = 0;

            if ( count == 0 )
            {
                return size;
            }

            var integer = new BigInteger();
            var leadingZero = Alphabet.LeadingZero;
            var previousDigit = leadingZero;

            for ( var i = index; i < count; i++ )
            {
                var digit = chars[i];
                var offset = Alphabet.GetOffset( digit );
                integer = ( integer * 58 ) + offset;

                if ( digit == leadingZero && previousDigit == leadingZero )
                {
                    ++size;
                }

                previousDigit = digit;
            }

            var bytes = integer.ToByteArray();

            count = bytes.Length;

            if ( IsLittleEndian && count > 1 )
            {
                Reverse( bytes, 0, count );
            }

            var start = 0;

            while ( start < count && bytes[start] == MinValue )
            {
                ++start;
            }

            var length = count - start;

            Copy( bytes, start, buffer, size, length );
            size += length;

            return size;
        }

        public override int GetBytes( char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool flush )
        {
            Copy( buffer, 0, bytes, byteIndex, size );
            return size;
        }

        public override void Reset()
        {
            base.Reset();
            buffer = null;
        }
    }
}