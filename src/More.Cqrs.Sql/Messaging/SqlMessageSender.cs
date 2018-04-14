// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain.Options;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a message sender backed by a SQL database.
    /// </summary>
    public class SqlMessageSender : IMessageSender
    {
        readonly ISqlMessageSerializer<IMessage> messageSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlMessageSender"/> class.
        /// </summary>
        /// <param name="configuration">The associated <see cref="SqlMessageQueueConfiguration">SQL queue configuration</see>.</param>
        public SqlMessageSender( SqlMessageQueueConfiguration configuration )
        {
            Arg.NotNull( configuration, nameof( configuration ) );

            Configuration = configuration;
            messageSerializer = configuration.MessageSerializer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlMessageSender"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string used by the message receiver.</param>
        public SqlMessageSender( string connectionString )
        {
            Arg.NotNullOrEmpty( connectionString, nameof( connectionString ) );

            var builder = new SqlMessageQueueConfigurationBuilder().HasConnectionString( connectionString );
            Configuration = builder.CreateConfiguration();
            messageSerializer = Configuration.MessageSerializer;
        }

        /// <summary>
        /// Gets the current SQL queue configuration.
        /// </summary>
        /// <value>The current <see cref="SqlMessageQueueConfiguration">SQL queue configuration</see>.</value>
        protected SqlMessageQueueConfiguration Configuration { get; }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="messages">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IMessageDescriptor">messages</see> to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual Task Send( IEnumerable<IMessageDescriptor> messages, CancellationToken cancellationToken )
        {
            Arg.NotNull( messages, nameof( messages ) );
            return Enqueue( messages, cancellationToken );
        }

        /// <summary>
        /// Enqueues the specified message.
        /// </summary>
        /// <param name="messageDescriptors">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IMessageDescriptor">message descriptors</see> to enqueue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        protected virtual async Task Enqueue( IEnumerable<IMessageDescriptor> messageDescriptors, CancellationToken cancellationToken )
        {
            Arg.NotNull( messageDescriptors, nameof( messageDescriptors ) );

            var clock = Configuration.Clock;
            var enqueueTime = clock.Now.UtcDateTime;

            using ( var connection = Configuration.CreateConnection() )
            {
                await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );

                using ( var transaction = connection.BeginTransaction() )
                {
                    foreach ( var messageDescriptor in messageDescriptors )
                    {
                        var item = new SqlMessageQueueItem()
                        {
                            EnqueueTime = enqueueTime,
                            DueTime = messageDescriptor.Options.GetDeliveryTime( clock ).UtcDateTime,
                            MessageType = messageDescriptor.MessageType,
                            Revision = messageDescriptor.Message.Revision,
                            Message = messageSerializer.Serialize( messageDescriptor.Message ),
                            Transaction = transaction,
                        };

                        await Configuration.Enqueue( connection, item, cancellationToken ).ConfigureAwait( false );
                    }

                    transaction.Commit();
                }
            }
        }
    }
}