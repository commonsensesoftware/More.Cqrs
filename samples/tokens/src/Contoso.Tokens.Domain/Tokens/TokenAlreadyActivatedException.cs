namespace Contoso.Domain.Tokens
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class TokenAlreadyActivatedException : TokenException
    {
        protected TokenAlreadyActivatedException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        public TokenAlreadyActivatedException() { }

        public TokenAlreadyActivatedException( string message ) : base( message ) { }

        public TokenAlreadyActivatedException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}