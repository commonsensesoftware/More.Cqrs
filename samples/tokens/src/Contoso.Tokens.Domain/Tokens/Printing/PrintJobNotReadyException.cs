namespace Contoso.Domain.Tokens.Printing
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class PrintJobNotReadyException : PrintJobException
    {
        protected PrintJobNotReadyException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        public PrintJobNotReadyException() { }

        public PrintJobNotReadyException( string message ) : base( message ) { }

        public PrintJobNotReadyException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}