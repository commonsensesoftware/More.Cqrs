namespace Contoso.Domain.Tokens.Printing
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class PrintJobCancellationException : PrintJobException
    {
        protected PrintJobCancellationException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        public PrintJobCancellationException() { }

        public PrintJobCancellationException( string message ) : base( message ) { }

        public PrintJobCancellationException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}