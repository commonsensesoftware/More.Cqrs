// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    /// <summary>
    /// Represents an in-memory <see cref="IMessageSender">message sender</see>.
    /// </summary>
    public class InMemoryMessageSender : IMessageSender, IDisposable
    {
        bool disposed;

        /// <summary>
        /// Finalizes an instance of the <see cref="InMemoryMessageSender"/> class.
        /// </summary>
        ~InMemoryMessageSender() => Dispose( false );

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryMessageSender"/> class.
        /// </summary>
        /// <param name="observer">The <see cref="IObserver{T}">observer</see> that receives sent messages.</param>
        /// <param name="pendingOperations">The object used to track <see cref="PendingOperations">pending operations</see>.</param>
        public InMemoryMessageSender( IObserver<IMessageDescriptor> observer, PendingOperations pendingOperations )
        {
            Observer = observer;
            PendingOperations = pendingOperations;
        }

        /// <summary>
        /// Gets the underlying observer to send messages to.
        /// </summary>
        /// <value>The <see cref="IObserver{T}">observer</see> that receives sent messages.</value>
        protected IObserver<IMessageDescriptor> Observer { get; }

        /// <summary>
        /// Gets the object that tracks the number of pending operations.
        /// </summary>
        /// <value>A <see cref="PendingOperations">pending operations</see> object.</value>
        protected PendingOperations PendingOperations { get; }

        /// <summary>
        /// Releases the managed and, optionally, the unmanaged resources used by the <see cref="InMemoryMessageSender"/> class.
        /// </summary>
        /// <param name="disposing">Indicates whether the object is being disposed.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;

            if ( disposing )
            {
                Observer.OnCompleted();
            }
            else
            {
                Observer?.OnCompleted();
            }
        }

        /// <summary>
        /// Releases the managed resources used by the <see cref="InMemoryMessageSender"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="messages">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IMessageDescriptor">messages</see> to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual Task Send( IEnumerable<IMessageDescriptor> messages, CancellationToken cancellationToken )
        {
            var values = messages.ToArray();

            PendingOperations.IncrementBy( values.Length );

            try
            {
                foreach ( var value in values )
                {
                    Observer.OnNext( value );
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch ( Exception error )
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Observer.OnError( error );
            }

            return CompletedTask;
        }
    }
}