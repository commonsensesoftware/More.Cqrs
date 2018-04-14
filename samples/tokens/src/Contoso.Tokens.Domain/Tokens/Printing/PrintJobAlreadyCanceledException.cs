namespace Contoso.Domain.Tokens.Printing
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class PrintJobAlreadyCanceledException : PrintJobException
    {
        protected PrintJobAlreadyCanceledException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        public PrintJobAlreadyCanceledException() { }

        public PrintJobAlreadyCanceledException( string message ) : base( message ) { }

        public PrintJobAlreadyCanceledException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}