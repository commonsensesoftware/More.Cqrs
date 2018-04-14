namespace Contoso.Domain.Tokens
{
    using System;
    using System.Diagnostics;
    using Text;
    using static System.Array;
    using static System.BitConverter;

    [DebuggerDisplay( "MintRequestId = {MintRequestId}, SequenceNumber = {SequenceNumber}" )]
    public sealed class TokenIdentifier
    {
        TokenIdentifier( Guid mintRequestId, long sequenceNumber )
        {
            MintRequestId = mintRequestId;
            SequenceNumber = sequenceNumber;
        }

        public Guid MintRequestId { get; }

        public long SequenceNumber { get; }

        public static TokenIdentifier Parse( string encodedTokenId )
        {
            if ( string.IsNullOrEmpty( encodedTokenId ) )
            {
                throw new ArgumentNullException( nameof( encodedTokenId ) );
            }

            var encoding = new Base58Encoding();
            var decodedBytes = encoding.GetBytes( encodedTokenId );
            var sequenceNumber = ToInt64( decodedBytes, 0 );
            var guidBytes = new byte[16];

            Copy( decodedBytes, 5, guidBytes, 0, 16 );

            var mintRequestId = new Guid( guidBytes );

            return new TokenIdentifier( mintRequestId, sequenceNumber );
        }
    }
}