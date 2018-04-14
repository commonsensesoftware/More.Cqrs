namespace Contoso.Domain
{
    using More.Domain.Messaging;
    using System;
    using Xunit.Abstractions;

    class MessageObserver : IObserver<IMessageDescriptor>
    {
        readonly ITestOutputHelper output;

        internal MessageObserver( ITestOutputHelper output ) => this.output = output;

        public void OnCompleted() { }

        public void OnError( Exception error ) { }

        public void OnNext( IMessageDescriptor value ) =>
            output.WriteLine( $"{value.Message.GetType().Name}, Id = {value.MessageId}, Correlation = {value.Message.CorrelationId}" );
    }
}