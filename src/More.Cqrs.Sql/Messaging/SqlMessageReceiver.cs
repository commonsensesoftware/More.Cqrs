﻿// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Reflection.Assembly;

    /// <summary>
    /// Represents a message receiver backed by a SQL database.
    /// </summary>
    public class SqlMessageReceiver : IMessageReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlMessageReceiver"/> class.
        /// </summary>
        /// <param name="configuration">The associated <see cref="SqlMessageQueueConfiguration">SQL queue configuration</see>.</param>
        /// <remarks>This constructor will use the name of entry assembly, calling assembly, and finally the defining assembly,
        /// in that precedence, as the basis of the subscription identifier.</remarks>
        public SqlMessageReceiver( SqlMessageQueueConfiguration configuration )
        {
            var assembly = GetEntryAssembly() ?? GetCallingAssembly() ?? GetType().GetTypeInfo().Assembly;

            Configuration = configuration;
            SubscriptionId = Uuid.FromString( assembly.GetName().Name! );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlMessageReceiver"/> class.
        /// </summary>
        /// <param name="streamName">The logical name of the associated stream. The name should be unique.</param>
        /// <param name="configuration">The associated <see cref="SqlMessageQueueConfiguration">SQL queue configuration</see>.</param>
        public SqlMessageReceiver( string streamName, SqlMessageQueueConfiguration configuration )
        {
            Configuration = configuration;
            SubscriptionId = Uuid.FromString( streamName );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlMessageReceiver"/> class.
        /// </summary>
        /// <param name="subscriptionId">The <see cref="Guid">GUID</see> that identifies the associated stream.</param>
        /// <param name="configuration">The associated <see cref="SqlMessageQueueConfiguration">SQL queue configuration</see>.</param>
        public SqlMessageReceiver( Guid subscriptionId, SqlMessageQueueConfiguration configuration )
        {
            Configuration = configuration;
            SubscriptionId = subscriptionId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlMessageReceiver"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string used by the message receiver.</param>
        public SqlMessageReceiver( string connectionString )
        {
            var assembly = GetEntryAssembly() ?? GetCallingAssembly() ?? GetType().GetTypeInfo().Assembly;
            var builder = new SqlMessageQueueConfigurationBuilder().HasConnectionString( connectionString );

            Configuration = builder.CreateConfiguration();
            SubscriptionId = Uuid.FromString( assembly.GetName().Name! );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlMessageReceiver"/> class.
        /// </summary>
        /// <param name="streamName">The logical name of the associated stream. The name should be unique.</param>
        /// <param name="connectionString">The connection string used by the message receiver.</param>
        public SqlMessageReceiver( string streamName, string connectionString )
        {
            var builder = new SqlMessageQueueConfigurationBuilder().HasConnectionString( connectionString );

            Configuration = builder.CreateConfiguration();
            SubscriptionId = Uuid.FromString( streamName );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlMessageReceiver"/> class.
        /// </summary>
        /// <param name="subscriptionId">The <see cref="Guid">GUID</see> that identifies the associated stream.</param>
        /// <param name="connectionString">The connection string used by the message receiver.</param>
        public SqlMessageReceiver( Guid subscriptionId, string connectionString )
        {
            var builder = new SqlMessageQueueConfigurationBuilder().HasConnectionString( connectionString );

            Configuration = builder.CreateConfiguration();
            SubscriptionId = subscriptionId;
        }

        /// <summary>
        /// Gets the subscription identifier for the message stream.
        /// </summary>
        /// <value>The subscription <see cref="Guid">identifier</see> for the message stream.</value>
        protected Guid SubscriptionId { get; }

        /// <summary>
        /// Gets the current SQL queue configuration.
        /// </summary>
        /// <value>The current <see cref="SqlMessageQueueConfiguration">SQL queue configuration</see>.</value>
        protected SqlMessageQueueConfiguration Configuration { get; }

        /// <summary>
        /// Deletes the corresponding message subscription, if any.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken">cancellation token</see> than can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        /// <remarks>If a corresponding subscription does not exist, this method performs no action.</remarks>
        public virtual Task DeleteSubscription( CancellationToken cancellationToken ) => Configuration.DeleteSubscription( SubscriptionId, cancellationToken );

        /// <summary>
        /// Subscribes to the message stream.
        /// </summary>
        /// <param name="observer">The <see cref="IObserver{T}"/> that receives messages from the stream.</param>
        /// <returns>A <see cref="IDisposable">disposable</see> object that can be used to the terminate the subscription.</returns>
        public IDisposable Subscribe( IObserver<IMessageDescriptor> observer ) => Subscribe( observer, Configuration.Clock.Now );

        /// <summary>
        /// Subscribes to the message stream.
        /// </summary>
        /// <param name="observer">The <see cref="IObserver{T}"/> that receives messages from the stream.</param>
        /// <param name="offset">The <see cref="TimeSpan">time</see> to offset from the current time to begin receiving messages from.</param>
        /// <returns>A <see cref="IDisposable">disposable</see> object that can be used to the terminate the subscription.</returns>
        public IDisposable Subscribe( IObserver<IMessageDescriptor> observer, TimeSpan offset ) => Subscribe( observer, Configuration.Clock.Now.Add( offset ) );

        /// <summary>
        /// Subscribes to the message stream.
        /// </summary>
        /// <param name="observer">The <see cref="IObserver{T}"/> that receives messages from the stream.</param>
        /// <param name="from">The <see cref="DateTimeOffset">date and time</see> in which to begin receiving messages from.</param>
        /// <returns>A <see cref="IDisposable">disposable</see> object that can be used to the terminate the subscription.</returns>
        public virtual IDisposable Subscribe( IObserver<IMessageDescriptor> observer, DateTimeOffset from ) => SqlMessagePump.StartNew( SubscriptionId, from, Configuration, observer );
    }
}