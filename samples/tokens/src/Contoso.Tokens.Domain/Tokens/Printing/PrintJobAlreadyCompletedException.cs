namespace Contoso.Domain.Tokens.Printing
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class PrintJobAlreadyCompletedException : PrintJobException
    {
        protected PrintJobAlreadyCompletedException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        public PrintJobAlreadyCompletedException() { }

        public PrintJobAlreadyCompletedException( string message ) : base( message ) { }

        public PrintJobAlreadyCompletedException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}