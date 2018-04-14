
namespace Contoso.Domain.Tokens
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class TokenNotInCirculationException : TokenException
    {
        protected TokenNotInCirculationException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        public TokenNotInCirculationException() { }

        public TokenNotInCirculationException( string message ) : base( message ) { }

        public TokenNotInCirculationException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}