namespace More.Domain.Example
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class MonogamyException : Exception
    {
        public MonogamyException() { }

        public MonogamyException( string message ) : base( message ) { }

        public MonogamyException( string message, Exception innerException ) : base( message, innerException ) { }

        protected MonogamyException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }
    }
}