
namespace Contoso.Domain.Tokens
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class TokenNotActivatedException : TokenException
    {
        protected TokenNotActivatedException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        public TokenNotActivatedException() { }

        public TokenNotActivatedException( string message ) : base( message ) { }

        public TokenNotActivatedException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}