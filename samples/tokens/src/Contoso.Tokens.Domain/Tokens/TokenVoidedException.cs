
namespace Contoso.Domain.Tokens
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class TokenVoidedException : TokenException
    {
        protected TokenVoidedException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        public TokenVoidedException() { }

        public TokenVoidedException( string message ) : base( message ) { }

        public TokenVoidedException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}