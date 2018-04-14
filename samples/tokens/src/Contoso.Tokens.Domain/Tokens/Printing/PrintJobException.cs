namespace Contoso.Domain.Tokens.Printing
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class PrintJobException : Exception
    {
        protected PrintJobException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        public PrintJobException() { }

        public PrintJobException( string message ) : base( message ) { }

        public PrintJobException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}