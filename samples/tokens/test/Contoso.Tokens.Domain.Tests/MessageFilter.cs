namespace Contoso.Domain
{
    using More.Domain;
    using More.Domain.Messaging;
    using More.Domain.Options;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using static System.TimeSpan;

    public class MessageFilter : IObserver<IMessageDescriptor>
    {
        readonly HashSet<Type> ignoredMessageTypes = new HashSet<Type>();
        readonly Dictionary<Type, TimeSpan> messageDelays = new Dictionary<Type, TimeSpan>();
        readonly IObserver<IMessageDescriptor> inner;
        readonly PendingOperations pendingOperations;
        readonly IClock clock;
        readonly ICollection<IMessageDescriptor> ignoredMessages;

        public MessageFilter(
            IObserver<IMessageDescriptor> inner,
            PendingOperations pendingOperations,
            IClock clock,
            ICollection<IMessageDescriptor> ignoredMessages )
        {
            this.inner = inner;
            this.pendingOperations = pendingOperations;
            this.clock = clock;
            this.ignoredMessages = ignoredMessages;
        }

        void IObserver<IMessageDescriptor>.OnCompleted() => inner.OnCompleted();

        void IObserver<IMessageDescriptor>.OnError( Exception error ) => inner.OnError( error );

        void IObserver<IMessageDescriptor>.OnNext( IMessageDescriptor value )
        {
            var key = value.Message.GetType();

            if ( ignoredMessageTypes.Contains( key ) )
            {
                ignoredMessages.Add( value );
                pendingOperations.Decrement();
            }
            else if ( messageDelays.TryGetValue( key, out var delay ) && delay > Zero )
            {
                OnNextWithDelay( value, delay );
            }
            else
            {
                var now = clock.Now;
                var deliveryTime = value.Options.GetDeliveryTime();

                if ( deliveryTime > now )
                {
                    OnNextWithDelay( value, deliveryTime - now );
                }
                else
                {
                    inner.OnNext( value );
                }
            }
        }

        async void OnNextWithDelay( IMessageDescriptor message, TimeSpan delay )
        {
            await Task.Delay( delay );
            inner.OnNext( message );
        }

        public MessageFilter Ignore<TMessage>() where TMessage : IMessage
        {
            ignoredMessageTypes.Add( typeof( TMessage ) );
            return this;
        }

        public Delay<TMessage> Delay<TMessage>() where TMessage : IMessage => new Delay<TMessage>( this, messageDelays );
    }
}