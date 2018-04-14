namespace Contoso.Domain.Tokens
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class TokenAlreadyReservedException : TokenException
    {
        protected TokenAlreadyReservedException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        public TokenAlreadyReservedException() { }

        public TokenAlreadyReservedException( string message ) : base( message ) { }

        public TokenAlreadyReservedException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}