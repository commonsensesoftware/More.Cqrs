// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;
    using System.Data.Common;
    using System.IO;

    /// <summary>
    /// Represent an item in a SQL database message queue.
    /// </summary>
    public class SqlMessageQueueItem : IDisposable
    {
        bool disposed;

        /// <summary>
        /// Finalizes an instance of the <see cref="SqlMessageQueueItem"/> class.
        /// </summary>
        ~SqlMessageQueueItem() => Dispose( false );

        /// <summary>
        /// Gets or sets the subscription identifier associated with the message.
        /// </summary>
        /// <value>The associated subscription identifier.</value>
        public Guid SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the identifier associated with the message.
        /// </summary>
        /// <value>The associated message identifier.</value>
        public Guid MessageId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the message was enqueued.
        /// </summary>
        /// <value>The <see cref="DateTime">date and time</see> in Universal Coordinated Time (UTC)
        /// when the message was enqueued.</value>
        public DateTime EnqueueTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the message should be processed.
        /// </summary>
        /// <value>The <see cref="DateTime">date and time</see> in Universal Coordinated Time (UTC)
        /// when the message should be processed.</value>
        public DateTime DueTime { get; set; }

        /// <summary>
        /// Gets or sets the number of attempts made to dequeue the item.
        /// </summary>
        /// <value>The number of attempts to dequeue the item. The default value is zero.</value>
        /// <remarks>This property can be used to detect poison messages.</remarks>
        public int DequeueAttempts { get; set; }

        /// <summary>
        /// Gets or sets the qualified name of the message type.
        /// </summary>
        /// <value>The qualified message type name.</value>
        public string MessageType { get; set; }

        /// <summary>
        /// Gets or sets the revision of the enqueued message.
        /// </summary>
        /// <value>The revision of the enqueued message.</value>
        /// <remarks>This revision can be used to handle different variations of individual
        /// message types over time. The default value is usually one.</remarks>
        public int Revision { get; set; }

        /// <summary>
        /// Gets or sets the serialized message stream.
        /// </summary>
        /// <value>The serialized message <see cref="Stream">stream</see>.</value>
        public Stream Message { get; set; }

        /// <summary>
        /// Deconstructs the message queue item into its constituent components.
        /// </summary>
        /// <param name="messageType">The message type component.</param>
        /// <param name="revision">The revision component.</param>
        /// <param name="message">The message component.</param>
        public void Deconstruct( out string messageType, out int revision, out Stream message )
        {
            messageType = MessageType;
            revision = Revision;
            message = Message;
        }

        /// <summary>
        /// Gets or sets the transaction associated with the item.
        /// </summary>
        /// <value>The <see cref="DbTransaction">database transaction</see> associated with the item.</value>
        /// <remarks>This property is typically used when dequeuing an item so that the item can be
        /// dequeued and re-enqueued, if necessary, within a single transaction. This property remains
        /// accessible for extensibility and customization. This property should only be used to flow an
        /// active transaction with an item.</remarks>
        public DbTransaction Transaction { get; set; }

        /// <summary>
        /// Releases the managed and, optionally, the unmanaged resources used by the <see cref="SqlMessageQueueItem"/> class.
        /// </summary>
        /// <param name="disposing">Indicates whether the object is being disposed.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;
            Message?.Dispose();
            Transaction = null;
        }

        /// <summary>
        /// Releases the managed resources used by the <see cref="SqlMessageQueueItem"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }
    }
}