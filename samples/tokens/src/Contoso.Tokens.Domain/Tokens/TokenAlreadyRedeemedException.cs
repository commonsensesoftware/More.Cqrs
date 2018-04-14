namespace Contoso.Domain.Tokens
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class TokenAlreadyRedeemedException : TokenException
    {
        protected TokenAlreadyRedeemedException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        public TokenAlreadyRedeemedException() { }

        public TokenAlreadyRedeemedException( string message ) : base( message ) { }

        public TokenAlreadyRedeemedException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}