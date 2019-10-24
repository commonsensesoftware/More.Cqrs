// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using Microsoft.ServiceBus.Messaging;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Threading.Tasks;
    using static System.Text.Encoding;

    /// <summary>
    /// Represents a <see cref="IMessageReceiver">message receiver</see> that is backed by Microsoft Service Bus.
    /// </summary>
    public class ServiceBusMessageReceiver : IMessageReceiver, IDisposable
    {
        readonly object syncRoot = new object();
        readonly List<IObserver<IMessageDescriptor>> observers = new List<IObserver<IMessageDescriptor>>();
        readonly SubscriptionClient client;
        bool disposed;
        volatile bool messagePumpStarted;

        /// <summary>
        /// Finalizes an instance of the <see cref="ServiceBusMessageReceiver"/> class.
        /// </summary>
        ~ServiceBusMessageReceiver() => Dispose( false );

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusMessageReceiver"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="ServiceBusConfiguration">Service Bus configuration</see> used by the message sender.</param>
        public ServiceBusMessageReceiver( ServiceBusConfiguration configuration )
        {
            client = configuration.CreateSubscriptionClient();
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the current Service Bus configuration.
        /// </summary>
        /// <value>The <see cref="ServiceBusConfiguration">Service Bus configuration</see> used by the message sender.</value>
        protected ServiceBusConfiguration Configuration { get; }

        /// <summary>
        /// Subscribes to the message stream.
        /// </summary>
        /// <param name="observer">The <see cref="IObserver{T}"/> that receives messages from the stream.</param>
        /// <returns>A <see cref="IDisposable">disposable</see> object that can be used to the terminate the subscription.</returns>
        public virtual IDisposable Subscribe( IObserver<IMessageDescriptor> observer )
        {
            if ( !messagePumpStarted )
            {
                messagePumpStarted = true;
                client.OnMessageAsync( OnMessage, Configuration.ReceiveOptions );
            }

            lock ( syncRoot )
            {
                observers.Add( observer );
            }

            return new Subscription( this, observer );
        }

        /// <summary>
        /// Creates and returns a message descriptor from the specified brokered message.
        /// </summary>
        /// <param name="brokeredMessage">The <see cref="BrokeredMessage">brokered message</see> to convert.</param>
        /// <returns>The converted <see cref="IMessageDescriptor">message descriptor</see>.</returns>
        protected virtual IMessageDescriptor FromBrokeredMessage( BrokeredMessage brokeredMessage )
        {
            var typeName = (string) brokeredMessage.Properties["Type"];
            var revision = (int) brokeredMessage.Properties["Revision"];
            var messageType = Configuration.MessageTypeResolver.ResolveType( typeName, revision );

            using ( var stream = brokeredMessage.GetBody<Stream>() )
            using ( var reader = new StreamReader( stream, UTF8 ) )
            {
                var message = (IMessage) Configuration.JsonSerializer.Deserialize( reader, messageType );
                return message.GetDescriptor();
            }
        }

        /// <summary>
        /// Releases the managed resources used by the <see cref="ServiceBusMessageReceiver"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Releases the unmanaged and, optionally the managed, resources used by the <see cref="ServiceBusMessageReceiver"/> class.
        /// </summary>
        /// <param name="disposing">Indicates whether the object is being disposed.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;

            if ( !disposing )
            {
                return;
            }

            lock ( syncRoot )
            {
                observers.Clear();
            }

            client.Close();
        }

        void Unsubscribe( IObserver<IMessageDescriptor> observer )
        {
            lock ( syncRoot )
            {
                observers.Remove( observer );
            }
        }

        async Task OnMessage( BrokeredMessage brokeredMessage )
        {
            var message = FromBrokeredMessage( brokeredMessage );
            var currentObservers = default( IEnumerable<IObserver<IMessageDescriptor>> );

            lock ( syncRoot )
            {
                currentObservers = observers.ToArray();
            }

            try
            {
                foreach ( var observer in currentObservers )
                {
                    observer.OnNext( message );
                }
            }
            catch ( TimeoutException error )
            {
                foreach ( var observer in currentObservers )
                {
                    observer.OnError( error );
                }
            }
            catch ( OperationCanceledException error )
            {
                foreach ( var observer in currentObservers )
                {
                    observer.OnError( error );
                }
            }
            catch
            {
                await brokeredMessage.AbandonAsync().ConfigureAwait( false );
                return;
            }

            await brokeredMessage.CompleteAsync().ConfigureAwait( false );
        }

        sealed class Subscription : IDisposable
        {
            readonly ServiceBusMessageReceiver receiver;
            readonly IObserver<IMessageDescriptor> observer;
            bool disposed;

            ~Subscription() => Dispose( false );

            internal Subscription( ServiceBusMessageReceiver receiver, IObserver<IMessageDescriptor> observer )
            {
                this.receiver = receiver;
                this.observer = observer;
            }

            void Dispose( bool disposing )
            {
                if ( disposed )
                {
                    return;
                }

                disposed = true;

                if ( disposing )
                {
                    receiver.Unsubscribe( observer );
                }
                else if ( observer != null )
                {
                    receiver?.Unsubscribe( observer );
                }
            }

            public void Dispose()
            {
                Dispose( true );
                GC.SuppressFinalize( this );
            }
        }
    }
}