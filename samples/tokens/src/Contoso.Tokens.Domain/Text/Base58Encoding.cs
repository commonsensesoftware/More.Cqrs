namespace Contoso.Text
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Text;

    public class Base58Encoding : Encoding
    {
        readonly Lazy<Base58Encoder> encoder;
        readonly Lazy<Base58Decoder> decoder;

        public Base58Encoding() : this( new BitcoinAlphabet() ) { }

        public Base58Encoding( Base58Alphabet alphabet )
        {
            Contract.Requires( alphabet != null );

            encoder = new Lazy<Base58Encoder>( () => new Base58Encoder( alphabet ) );
            decoder = new Lazy<Base58Decoder>( () => new Base58Decoder( alphabet ) );
        }

        public override string EncodingName => "Base58";

        public override bool IsSingleByte => false;

        public override Encoder GetEncoder() => encoder.Value;

        public override Decoder GetDecoder() => decoder.Value;

        public override int GetByteCount( char[] chars, int index, int count ) =>
            GetEncoder().GetByteCount( chars, index, count, flush: true );

        public override int GetBytes( char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex ) =>
            GetEncoder().GetBytes( chars, charIndex, charCount, bytes, byteIndex, flush: true );

        public override int GetCharCount( byte[] bytes, int index, int count ) =>
            GetDecoder().GetCharCount( bytes, index, count, flush: true );

        public override int GetChars( byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex ) =>
            GetDecoder().GetChars( bytes, byteIndex, byteCount, chars, charIndex, flush: true );

        public override int GetMaxByteCount( int charCount ) => charCount;

        public override int GetMaxCharCount( int byteCount ) => byteCount;
    }
}