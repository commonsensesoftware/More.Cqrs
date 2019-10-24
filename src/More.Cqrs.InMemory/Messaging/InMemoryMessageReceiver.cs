// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

#pragma warning disable CA1716 // Identifiers should not match keywords

namespace More.Domain.Messaging
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an in-memory <see cref="IMessageReceiver">message receiver</see>.
    /// </summary>
    public class InMemoryMessageReceiver : IMessageReceiver, IObserver<IMessageDescriptor>, IDisposable
    {
        readonly object syncRoot = new object();
        readonly List<ReceiveMessageStream> streams = new List<ReceiveMessageStream>();
        bool disposed;

        /// <summary>
        /// Finalizes an instance of the <see cref="InMemoryMessageReceiver"/> class.
        /// </summary>
        ~InMemoryMessageReceiver() => Dispose( false );

        /// <summary>
        /// Releases the managed and, optionally, the unmanaged resources used by the <see cref="InMemoryMessageReceiver"/> class.
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
                foreach ( var stream in streams )
                {
                    stream.Dispose();
                }

                streams.Clear();
            }
        }

        /// <summary>
        /// Releases the managed resources used by the <see cref="InMemoryMessageReceiver"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Occurs when the message receiver will not receive any additional messages.
        /// </summary>
        public virtual void OnCompleted() => Dispose();

        /// <summary>
        /// Occurs when an error occurs while receiving a message.
        /// </summary>
        /// <param name="error">The <see cref="Exception">error</see> that occurred while receiving the message.</param>
        /// <remarks>The default implementation re-throws the <paramref name="error"/>.</remarks>
        public virtual void OnError( Exception error ) => error?.Rethrow();

        /// <summary>
        /// Occurs when a new message is received.
        /// </summary>
        /// <param name="value">The <see cref="IMessageDescriptor">message</see> received.</param>
        public virtual void OnNext( IMessageDescriptor value )
        {
            var currentStreams = default( IEnumerable<ReceiveMessageStream> );

            lock ( syncRoot )
            {
                currentStreams = streams.ToArray();
            }

            foreach ( var stream in currentStreams )
            {
                stream.Receive( value );
            }
        }

        /// <summary>
        /// Subscribes to the message stream.
        /// </summary>
        /// <param name="observer">The <see cref="IObserver{T}"/> that receives messages from the stream.</param>
        /// <returns>A <see cref="IDisposable">disposable</see> object that can be used to the terminate the subscription.</returns>
        public virtual IDisposable Subscribe( IObserver<IMessageDescriptor> observer )
        {
            var stream = new ReceiveMessageStream();

            lock ( syncRoot )
            {
                streams.Add( stream );
            }

            return stream.Subscribe( observer );
        }

        sealed class ReceiveMessageStream : IObservable<IMessageDescriptor>, IDisposable
        {
            readonly BlockingCollection<IMessageDescriptor> messages = new BlockingCollection<IMessageDescriptor>();
            bool disposed;

            internal void Receive( IMessageDescriptor message ) => messages.Add( message );

            public IDisposable Subscribe( IObserver<IMessageDescriptor> observer ) => MessagePump.StartNew( messages, observer );

            public void Dispose()
            {
                if ( disposed )
                {
                    return;
                }

                disposed = true;
                messages.CompleteAdding();
                messages.Dispose();
            }
        }
    }
}