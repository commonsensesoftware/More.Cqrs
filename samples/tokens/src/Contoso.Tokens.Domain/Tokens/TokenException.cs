namespace Contoso.Domain.Tokens
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class TokenException : Exception
    {
        protected TokenException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        public TokenException() { }

        public TokenException( string message ) : base( message ) { }

        public TokenException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}