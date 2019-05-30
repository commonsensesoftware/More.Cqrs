// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using Microsoft.Database.Isam;
    using More.Domain.Options;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    /// <summary>
    /// Represents a message sender backed by an ISAM database.
    /// </summary>
    public class IsamMessageSender : IMessageSender
    {
        readonly IIsamMessageSerializer<IMessage> messageSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="IsamMessageSender"/> class.
        /// </summary>
        /// <param name="configuration">The associated <see cref="IsamMessageQueueConfiguration">Isam queue configuration</see>.</param>
        public IsamMessageSender( IsamMessageQueueConfiguration configuration )
        {
            Arg.NotNull( configuration, nameof( configuration ) );

            Configuration = configuration;
            messageSerializer = configuration.MessageSerializer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IsamMessageSender"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string used by the message receiver.</param>
        public IsamMessageSender( string connectionString )
        {
            Arg.NotNullOrEmpty( connectionString, nameof( connectionString ) );

            var builder = new IsamMessageQueueConfigurationBuilder().HasConnectionString( connectionString );
            Configuration = builder.CreateConfiguration();
            messageSerializer = Configuration.MessageSerializer;
        }

        /// <summary>
        /// Gets the current ISAM queue configuration.
        /// </summary>
        /// <value>The current <see cref="IsamMessageQueueConfiguration">ISAM queue configuration</see>.</value>
        protected IsamMessageQueueConfiguration Configuration { get; }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="messages">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IMessageDescriptor">messages</see> to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual Task Send( IEnumerable<IMessageDescriptor> messages, CancellationToken cancellationToken )
        {
            Arg.NotNull( messages, nameof( messages ) );
            Enqueue( messages, cancellationToken );
            return CompletedTask;
        }

        /// <summary>
        /// Enqueues the specified messages.
        /// </summary>
        /// <param name="messageDescriptors">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IMessageDescriptor">message descriptors</see> to enqueue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        protected virtual void Enqueue( IEnumerable<IMessageDescriptor> messageDescriptors, CancellationToken cancellationToken )
        {
            Arg.NotNull( messageDescriptors, nameof( messageDescriptors ) );

            var clock = Configuration.Clock;
            var enqueueTime = clock.Now.UtcDateTime;
            var connection = Configuration.CreateConnection();

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                using ( var transaction = new IsamTransaction( session ) )
                {
                    foreach ( var messageDescriptor in messageDescriptors )
                    {
                        var item = new IsamMessageQueueItem()
                        {
                            EnqueueTime = enqueueTime,
                            DueTime = messageDescriptor.Options.GetDeliveryTime( clock ).UtcDateTime,
                            MessageType = messageDescriptor.MessageType,
                            Revision = messageDescriptor.Message.Revision,
                            Message = messageSerializer.Serialize( messageDescriptor.Message ),
                            Transaction = transaction,
                        };

                        Configuration.Enqueue( database, item );

                        if ( cancellationToken.IsCancellationRequested )
                        {
                            transaction.Rollback();
                            break;
                        }
                    }

                    if ( !cancellationToken.IsCancellationRequested )
                    {
                        transaction.Commit();
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}