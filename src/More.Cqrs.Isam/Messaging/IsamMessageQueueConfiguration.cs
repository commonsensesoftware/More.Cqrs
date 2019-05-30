// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using Microsoft.Database.Isam;
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using static Microsoft.Database.Isam.BoundCriteria;
    using static Microsoft.Database.Isam.ColumnFlags;
    using static Microsoft.Database.Isam.IndexFlags;

    /// <summary>
    /// Represents the configuration for an ISAM database message queue.
    /// </summary>
    public class IsamMessageQueueConfiguration
    {
        readonly string connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="IsamMessageQueueConfiguration"/> class.
        /// </summary>
        /// <param name="clock">The <see cref="IClock">clock</see> used for temporal calculations.</param>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="messageQueueTableName">The identifier of the message queue table.</param>
        /// <param name="subscriptionTableName">The identifier of the subscription table.</param>
        /// <param name="subscriptionQueueTableName">The identifier of the subscription queue table.</param>
        /// <param name="messageTypeResolver">The <see cref="IMessageTypeResolver">message type resolver</see> used to deserialize messages.</param>
        /// <param name="serializerFactory">The <see cref="IIsamMessageSerializerFactory">factory</see> used to create new message serializers.</param>
        public IsamMessageQueueConfiguration(
            IClock clock,
            string connectionString,
            string messageQueueTableName,
            string subscriptionTableName,
            string subscriptionQueueTableName,
            IMessageTypeResolver messageTypeResolver,
            IIsamMessageSerializerFactory serializerFactory )
        {
            Arg.NotNullOrEmpty( connectionString, nameof( connectionString ) );
            Arg.NotNullOrEmpty( messageQueueTableName, nameof( messageQueueTableName ) );
            Arg.NotNullOrEmpty( subscriptionTableName, nameof( subscriptionTableName ) );
            Arg.NotNullOrEmpty( subscriptionQueueTableName, nameof( subscriptionQueueTableName ) );
            Arg.NotNull( messageTypeResolver, nameof( messageTypeResolver ) );
            Arg.NotNull( serializerFactory, nameof( serializerFactory ) );

            Clock = clock;
            this.connectionString = connectionString;
            MessageQueueTableName = messageQueueTableName;
            SubscriptionTableName = subscriptionTableName;
            SubscriptionQueueTableName = subscriptionQueueTableName;
            MessageTypeResolver = messageTypeResolver;
            MessageSerializer = serializerFactory.NewSerializer<IMessage>();
        }

        /// <summary>
        /// Gets the configured clock.
        /// </summary>
        /// <value>The configured <see cref="IClock">clock</see>.</value>
        public IClock Clock { get; }

        /// <summary>
        /// Gets the identifier of the message queue table.
        /// </summary>
        /// <vaule>The identifier of the message queue table.</vaule>
        public string MessageQueueTableName { get; }

        /// <summary>
        /// Gets the identifier of the subscription table.
        /// </summary>
        /// <vaule>The identifier of the subscription table.</vaule>
        public string SubscriptionTableName { get; }

        /// <summary>
        /// Gets the identifier of the subscription queue table.
        /// </summary>
        /// <vaule>The identifier of the subscription queue table.</vaule>
        public string SubscriptionQueueTableName { get; }

        /// <summary>
        /// Gets the message type resolver used to deserialize messages.
        /// </summary>
        /// <value>The <see cref="IMessageTypeResolver">message type resolver</see> used to deserialize messages.</value>
        public IMessageTypeResolver MessageTypeResolver { get; }

        /// <summary>
        /// Gets the serializer used to serialize and deserialize messages.
        /// </summary>
        /// <value>The <see cref="IIsamMessageSerializer{TMessage}">serializer</see> used to serialize and deserialize messages.</value>
        public IIsamMessageSerializer<IMessage> MessageSerializer { get; }

        /// <summary>
        /// Create a new database connection.
        /// </summary>
        /// <returns>A new, configured <see cref="IsamConnection">database connection</see>.</returns>
        public virtual IsamConnection CreateConnection() => new IsamConnection( connectionString );

        /// <summary>
        /// Creates the message queue tables.
        /// </summary>
        public virtual void CreateTables()
        {
            var connection = CreateConnection();
            var tables = new[] { NewMessageQueueTable(), NewSubscriptionTable(), NewSubscriptionQueueTable() };

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                using ( var transaction = new IsamTransaction( session ) )
                {
                    for ( var i = 0; i < tables.Length; i++ )
                    {
                        var table = tables[i];

                        if ( !database.Exists( table.Name ) )
                        {
                            database.CreateTable( table );
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Creates a message queue subscription.
        /// </summary>
        /// <param name="subscriptionId">The associated subscription identifier.</param>
        /// <param name="creationTime">The <see cref="DateTimeOffset">date and time</see> when the subscription is to be created.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        public virtual void CreateSubscription( Guid subscriptionId, DateTimeOffset creationTime, CancellationToken cancellationToken )
        {
            var connection = CreateConnection();

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                {
                    CreateSubscription( database, subscriptionId, creationTime, cancellationToken );
                }
            }
        }

        /// <summary>
        /// Creates a message queue subscription using the specified connection.
        /// </summary>
        /// <param name="database">The <see cref="IsamDatabase">database</see> to create a subscription with.</param>
        /// <param name="subscriptionId">The associated subscription identifier.</param>
        /// <param name="creationTime">The <see cref="DateTimeOffset">date and time</see> when the subscription is to be created.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        public virtual void CreateSubscription( IsamDatabase database, Guid subscriptionId, DateTimeOffset creationTime, CancellationToken cancellationToken )
        {
            Arg.NotNull( database, nameof( database ) );

            var now = DateTime.UtcNow;

            using ( var subscription = database.OpenCursor( SubscriptionTableName ) )
            {
                subscription.SetCurrentIndex( "PK_" + SubscriptionTableName );

                if ( subscription.GotoKey( Key.Compose( subscriptionId ) ) )
                {
                    return;
                }

                using ( var transaction = new IsamTransaction( database.IsamSession ) )
                using ( var messages = database.OpenCursor( MessageQueueTableName ) )
                using ( var copiedMessages = database.OpenCursor( SubscriptionQueueTableName ) )
                {
                    subscription.BeginEditForInsert();

                    var newSubscription = subscription.EditRecord;

                    newSubscription["SubscriptionId"] = subscriptionId;
                    newSubscription["CreationTime"] = creationTime.UtcDateTime;
                    subscription.AcceptChanges();
                    cancellationToken.ThrowIfCancellationRequested();
                    messages.SetCurrentIndex( "PK_" + MessageQueueTableName );
                    messages.FindRecords( MatchCriteria.GreaterThanOrEqualTo, Key.ComposeWildcard( now ) );

                    foreach ( var message in messages )
                    {
                        copiedMessages.BeginEditForInsert();

                        var copiedMessage = copiedMessages.EditRecord;

                        copiedMessage["SubscriptionId"] = subscriptionId;
                        copiedMessage["MessageId"] = message["MessageId"][0];
                        copiedMessage["EnqueueTime"] = message["EnqueueTime"][0];
                        copiedMessage["DequeueAttempts"] = 0;
                        copiedMessage["DueTime"] = message["DueTime"][0];
                        copiedMessage["Type"] = message["Type"][0];
                        copiedMessage["Revision"] = message["Revision"][0];
                        copiedMessage["Message"] = message["Message"][0];
                        copiedMessages.AcceptChanges();
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Deletes a message queue subscription using the specified subscription identifier.
        /// </summary>
        /// <param name="subscriptionId">The associated subscription identifier.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        public virtual void DeleteSubscription( Guid subscriptionId, CancellationToken cancellationToken )
        {
            var connection = CreateConnection();

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                {
                    DeleteSubscription( database, subscriptionId, cancellationToken );
                }
            }
        }

        /// <summary>
        /// Deletes a message queue subscription using the specified connection and subscription identifier.
        /// </summary>
        /// <param name="database">The <see cref="IsamDatabase">database</see> to create a subscription with.</param>
        /// <param name="subscriptionId">The associated subscription identifier.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        public virtual void DeleteSubscription( IsamDatabase database, Guid subscriptionId, CancellationToken cancellationToken )
        {
            Arg.NotNull( database, nameof( database ) );

            using ( var subscription = database.OpenCursor( SubscriptionTableName ) )
            {
                subscription.SetCurrentIndex( "PK_" + SubscriptionTableName );

                if ( !subscription.GotoKey( Key.Compose( subscriptionId ) ) )
                {
                    return;
                }

                using ( var transaction = new IsamTransaction( database.IsamSession ) )
                using ( var messages = database.OpenCursor( SubscriptionQueueTableName ) )
                {
                    messages.SetCurrentIndex( "PK_" + SubscriptionQueueTableName );
                    messages.FindRecords( MatchCriteria.EqualTo, Key.ComposeWildcard( subscriptionId ) );

                    while ( messages.MoveNext() )
                    {
                        messages.Delete();
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    subscription.Delete();
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Enqueues the specified item using the provided database.
        /// </summary>
        /// <param name="database">The <see cref="IsamDatabase">database</see> to enqueue an item with.</param>
        /// <param name="item">The <see cref="IsamMessageQueueItem">item</see> to enqueue.</param>
        public virtual void Enqueue( IsamDatabase database, IsamMessageQueueItem item )
        {
            Arg.NotNull( database, nameof( database ) );
            Arg.NotNull( item, nameof( item ) );

            if ( item.MessageId == Guid.Empty )
            {
                item.MessageId = Uuid.NewSequentialId();
            }

            var newMessage = item.SubscriptionId == Guid.Empty;

            if ( newMessage )
            {
                using ( var messages = database.OpenCursor( MessageQueueTableName ) )
                {
                    Enqueue( messages, item );

                    using ( var subscriptions = database.OpenCursor( SubscriptionTableName ) )
                    {
                        while ( subscriptions.MoveNext() )
                        {
                            item.SubscriptionId = (Guid) subscriptions.Record["SubscriptionId"];

                            using ( var queue = database.OpenCursor( SubscriptionQueueTableName ) )
                            {
                                Enqueue( queue, item );
                            }
                        }
                    }
                }
            }
            else
            {
                using ( var queue = database.OpenCursor( SubscriptionQueueTableName ) )
                {
                    Enqueue( queue, item );
                }
            }
        }

        /// <summary>
        /// Enqueues the specified item using the provided cursor.
        /// </summary>
        /// <param name="cursor">The <see cref="Cursor">cursor</see> to enqueue an item with.</param>
        /// <param name="item">The <see cref="IsamMessageQueueItem">item</see> to enqueue.</param>
        public virtual void Enqueue( Cursor cursor, IsamMessageQueueItem item )
        {
            Arg.NotNull( cursor, nameof( cursor ) );
            Arg.NotNull( item, nameof( item ) );

            cursor.BeginEditForInsert();

            var message = cursor.EditRecord;

            message["MessageId"] = item.MessageId;
            message["EnqueueTime"] = item.EnqueueTime;
            message["DueTime"] = item.DueTime;
            message["Type"] = item.MessageType;
            message["Revision"] = item.Revision;
            message["Message"] = item.Message.ToBytes();

            if ( item.SubscriptionId != Guid.Empty )
            {
                message["SubscriptionId"] = item.SubscriptionId;
                message["DequeueAttempts"] = item.DequeueAttempts;
            }

            cursor.AcceptChanges();
        }

        /// <summary>
        /// Dequeues a single item using the specified connection.
        /// </summary>
        /// <param name="database">The <see cref="IsamDatabase">database</see> to dequeue an item with.</param>
        /// <param name="subscriptionId">The associated subscription identifier.</param>
        /// <param name="dueTime">The due <see cref="DateTimeOffset">date and time</see> for the next item to be dequeued.</param>
        /// <returns>The <see cref="IIsamDequeueOperation">dequeue operation</see>.</returns>
        public virtual IIsamDequeueOperation Dequeue( IsamDatabase database, Guid subscriptionId, DateTimeOffset dueTime )
        {
            Arg.NotNull( database, nameof( database ) );
            Contract.Ensures( Contract.Result<Task<IIsamDequeueOperation>>() != null );

            var firstKey = Key.Compose( subscriptionId, DateTime.MinValue, 0 );
            var lastKey = Key.Compose( subscriptionId, dueTime.UtcDateTime, int.MaxValue );

            using ( var cursor = database.OpenCursor( SubscriptionQueueTableName ) )
            {
                cursor.SetCurrentIndex( "IX_" + SubscriptionQueueTableName );
                cursor.FindRecordsBetween( firstKey, Inclusive, lastKey, Inclusive );

                if ( !cursor.MoveNext() )
                {
                    return IsamDequeueOperation.Empty;
                }

                var message = cursor.Record;
                var item = new IsamMessageQueueItem
                {
                    SubscriptionId = subscriptionId,
                    MessageId = (Guid) message["MessageId"],
                    EnqueueTime = (DateTime) message["EnqueueTime"],
                    DequeueAttempts = (int) message["DequeueAttempts"],
                    DueTime = (DateTime) message["DueTime"],
                    MessageType = (string) message["Type"],
                    Revision = (int) message["Revision"],
                    Message = new MemoryStream( (byte[]) message["Message"], writable: false ),
                    Transaction = new IsamTransaction( database.IsamSession, join: true ),
                };

                cursor.Delete();

                return new IsamDequeueOperation( item );
            }
        }

        /// <summary>
        /// Creates and returns a table definition for a message queue.
        /// </summary>
        /// <returns>A new <see cref="TableDefinition">table definition</see>.</returns>
        protected virtual TableDefinition NewMessageQueueTable()
        {
            var table = new TableDefinition( MessageQueueTableName );
            var columns = table.Columns;
            var primaryKeyIndex = new IndexDefinition( "PK_" + table.Name )
            {
                Flags = Primary | Unique | DisallowNull | DisallowTruncation,
                KeyColumns =
                {
                    new KeyColumn( "EnqueueTime", isAscending: true ),
                    new KeyColumn( "MessageId", isAscending: true ),
                },
            };

            columns.Add( new ColumnDefinition( "MessageId", typeof( Guid ), Fixed | NonNull ) );
            columns.Add( new ColumnDefinition( "EnqueueTime", typeof( DateTime ), Fixed | NonNull ) );
            columns.Add( new ColumnDefinition( "DueTime", typeof( DateTime ), Fixed | NonNull ) );
            columns.Add( new ColumnDefinition( "Type", typeof( string ), Variable | NonNull ) );
            columns.Add( new ColumnDefinition( "Revision", typeof( int ), Fixed | NonNull ) { DefaultValue = 1 } );
            columns.Add( new ColumnDefinition( "Message", typeof( byte[] ), NonNull ) );
            table.Indices.Add( primaryKeyIndex );

            return table;
        }

        /// <summary>
        /// Creates and returns a table definition for subscriptions.
        /// </summary>
        /// <returns>A new <see cref="TableDefinition">table definition</see>.</returns>
        protected virtual TableDefinition NewSubscriptionTable()
        {
            var table = new TableDefinition( SubscriptionTableName );
            var columns = table.Columns;
            var primaryKeyIndex = new IndexDefinition( "PK_" + table.Name )
            {
                Flags = Primary | Unique | DisallowNull | DisallowTruncation,
                KeyColumns = { new KeyColumn( "SubscriptionId", isAscending: true ) },
            };

            columns.Add( new ColumnDefinition( "SubscriptionId", typeof( Guid ), Fixed | NonNull ) );
            columns.Add( new ColumnDefinition( "CreationTime", typeof( DateTime ), Fixed | NonNull ) );
            table.Indices.Add( primaryKeyIndex );

            return table;
        }

        /// <summary>
        /// Creates and returns a table definition for a subscription queue.
        /// </summary>
        /// <returns>A new <see cref="TableDefinition">table definition</see>.</returns>
        protected virtual TableDefinition NewSubscriptionQueueTable()
        {
            var table = new TableDefinition( SubscriptionQueueTableName );
            var columns = table.Columns;
            var indices = table.Indices;
            var primaryKeyIndex = new IndexDefinition( "PK_" + table.Name )
            {
                Flags = Primary | Unique | DisallowNull | DisallowTruncation,
                KeyColumns =
                {
                    new KeyColumn( "SubscriptionId", isAscending: true ),
                    new KeyColumn( "MessageId", isAscending: true ),
                },
            };
            var dequeueIndex = new IndexDefinition( "IX_" + table.Name )
            {
                Flags = DisallowNull | DisallowTruncation,
                KeyColumns =
                {
                    new KeyColumn( "SubscriptionId", isAscending: true ),
                    new KeyColumn( "DueTime", isAscending: true ),
                    new KeyColumn( "DequeueAttempts", isAscending: true ),
                },
            };

            columns.Add( new ColumnDefinition( "SubscriptionId", typeof( Guid ), Fixed | NonNull ) );
            columns.Add( new ColumnDefinition( "MessageId", typeof( Guid ), Fixed | NonNull ) );
            columns.Add( new ColumnDefinition( "EnqueueTime", typeof( DateTime ), Fixed | NonNull ) );
            columns.Add( new ColumnDefinition( "DequeueAttempts", typeof( int ), Fixed | NonNull ) { DefaultValue = 0 } );
            columns.Add( new ColumnDefinition( "DueTime", typeof( DateTime ), Fixed | NonNull ) );
            columns.Add( new ColumnDefinition( "Type", typeof( string ), Variable | NonNull ) );
            columns.Add( new ColumnDefinition( "Revision", typeof( int ), Fixed | NonNull ) { DefaultValue = 1 } );
            columns.Add( new ColumnDefinition( "Message", typeof( byte[] ), NonNull ) );
            indices.Add( primaryKeyIndex );
            indices.Add( dequeueIndex );

            return table;
        }
    }
}