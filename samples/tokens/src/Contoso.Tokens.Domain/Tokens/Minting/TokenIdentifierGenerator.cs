namespace Contoso.Domain.Tokens.Minting
{
    using System;
    using System.Diagnostics.Contracts;
    using Text;
    using static System.Array;
    using static System.BitConverter;

    public class TokenIdentifierGenerator
    {
        const int IdSize = 5;
        const int GuidSize = 16;
        const int BufferSize = IdSize + GuidSize;
        const long MaxValue = 0x10000000000L;
        readonly Base58Encoding encoding = new Base58Encoding();
        readonly byte[] buffer = new byte[BufferSize];
        long current;

        public TokenIdentifierGenerator( Guid mintRequestId, long start = 1L )
        {
            Contract.Requires( start >= 1 && start <= MaxValue );

            current = start;
            Copy( mintRequestId.ToByteArray(), 0, buffer, IdSize, GuidSize );
        }

        public string Next()
        {
            Increment();
            return encoding.GetString( buffer );
        }

        public byte[] NextBytes()
        {
            Increment();
            var bytes = new byte[BufferSize];
            Copy( buffer, bytes, BufferSize );
            return bytes;
        }

        void Increment()
        {
            if ( current == MaxValue )
            {
                throw new InvalidOperationException( $"The maximum supported token identifier of {MaxValue.ToString( "N0" )} has been reached." );
            }

            Copy( GetBytes( current++ ), buffer, IdSize );
        }
    }
}