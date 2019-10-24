// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

#pragma warning disable CA1716 // Identifiers should not match keywords

namespace More.Domain.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Represents an object that forwards messages from one or more message streams to another.
    /// </summary>
    public class MessageForwarder : IObserver<IMessageDescriptor>, IDisposable
    {
        bool disposed;
        IDisposable? subscription;
        CancellationTokenSource source = new CancellationTokenSource();

        /// <summary>
        /// Finalizes an instance of the <see cref="MessageForwarder"/> class.
        /// </summary>
        ~MessageForwarder() => Dispose( false );

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageForwarder"/> class.
        /// </summary>
        /// <param name="messageSender">The <see cref="IMessageSender">message sender</see> used to schedule messages with.</param>
        public MessageForwarder( IMessageSender messageSender ) => MessageSender = messageSender;

        /// <summary>
        /// Gets the sender to forward messages to.
        /// </summary>
        /// <value>The <see cref="IMessageSender">sender</see> to forward messages to.</value>
        protected IMessageSender MessageSender { get; }

        /// <summary>
        /// Begins forwarding messages from the specified receivers.
        /// </summary>
        /// <param name="messageReceivers">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IMessageReceiver">message receivers</see>
        /// to subscribe to for message forwarding.</param>
        /// <remarks>Messages are forwarded until the object is disposed.</remarks>
        public void ForwardFrom( IEnumerable<IMessageReceiver> messageReceivers ) => ForwardFrom( messageReceivers.ToArray() );

        /// <summary>
        /// Begins forwarding messages from the specified receivers.
        /// </summary>
        /// <param name="messageReceivers">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IMessageReceiver">message receivers</see>
        /// to subscribe to for message forwarding.</param>
        /// <remarks>Messages are forwarded until the object is disposed.</remarks>
        public virtual void ForwardFrom( params IMessageReceiver[] messageReceivers )
        {
            if ( source != null )
            {
                source.Cancel();
                source.Dispose();
            }

            subscription?.Dispose();

            if ( messageReceivers.Length == 0 )
            {
                return;
            }

            subscription = new CompositeDisposable( messageReceivers.Select( receiver => receiver.Subscribe( this ) ) );
            source = new CancellationTokenSource();
            CancellationToken = source.Token;
        }

        /// <summary>
        /// Releases the managed and, optionally, the unmanaged resources used by the <see cref="MessageForwarder"/> class.
        /// </summary>
        /// <param name="disposing">Indicates whether the object is being disposed.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;
            subscription?.Dispose();

            if ( source != null )
            {
                source.Cancel();
                source.Dispose();
            }
        }

        /// <summary>
        /// Releases the managed resources used by the <see cref="MessageForwarder"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Gets the cancellation token used cancel pending asynchronous operations.
        /// </summary>
        /// <value>The <see cref="CancellationToken">cancellation token</see> used cancel pending asynchronous operations.</value>
        protected CancellationToken CancellationToken { get; private set; }

        /// <summary>
        /// Occurs when the end of all the underlying message streams has been reached.
        /// </summary>
        public virtual void OnCompleted() => source?.Cancel();

        /// <summary>
        /// Occurs when an error is encountered.
        /// </summary>
        /// <param name="error">The error that occurred.</param>
        public virtual void OnError( Exception error ) { }

        /// <summary>
        /// Occurs when a message has been received.
        /// </summary>
        /// <param name="value">The <see cref="IMessageDescriptor">message</see> that was received.</param>
        public virtual async void OnNext( IMessageDescriptor value )
        {
            try
            {
                await MessageSender.Send( value, CancellationToken ).ConfigureAwait( false );
            }
            catch ( OperationCanceledException )
            {
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch ( Exception error )
#pragma warning restore CA1031 // Do not catch general exception types
            {
                OnError( error );
            }
        }

        sealed class CompositeDisposable : IDisposable
        {
            readonly IDisposable[] disposables;
            bool disposed;

            ~CompositeDisposable() => Dispose( false );

            internal CompositeDisposable( IEnumerable<IDisposable> disposables ) => this.disposables = disposables.ToArray();

            void Dispose( bool disposing )
            {
                if ( disposed )
                {
                    return;
                }

                disposed = true;

                if ( disposing )
                {
                    foreach ( var disposable in disposables )
                    {
                        disposable.Dispose();
                    }
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