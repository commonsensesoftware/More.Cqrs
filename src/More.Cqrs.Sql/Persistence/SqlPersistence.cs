// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Persistence
{
    using More.Domain.Events;
    using More.Domain.Messaging;
    using More.Domain.Options;
    using More.Domain.Sagas;
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.TimeSpan;

    /// <summary>
    /// Represents persistence backed by a relational database.
    /// </summary>
    public class SqlPersistence : IPersistence
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlPersistence"/> class.
        /// </summary>
        /// <param name="messageQueueConfiguration">The <see cref="SqlMessageQueueConfiguration">configuration</see> for queued messages.</param>
        /// <param name="eventStoreConfiguration">The <see cref="SqlEventStoreConfiguration">configuration</see> for events.</param>
        /// <param name="sagaStorageConfiguration">The <see cref="SqlSagaStorageConfiguration">configuration</see> for sagas.</param>
        public SqlPersistence(
            SqlMessageQueueConfiguration messageQueueConfiguration,
            SqlEventStoreConfiguration eventStoreConfiguration,
            SqlSagaStorageConfiguration sagaStorageConfiguration )
        {
            Arg.NotNull( messageQueueConfiguration, nameof( messageQueueConfiguration ) );
            Arg.NotNull( eventStoreConfiguration, nameof( eventStoreConfiguration ) );
            Arg.NotNull( sagaStorageConfiguration, nameof( sagaStorageConfiguration ) );

            Configuration = new PersistenceConfiguration( messageQueueConfiguration, eventStoreConfiguration, sagaStorageConfiguration );
        }

        /// <summary>
        /// Gets the configuration used by persistence.
        /// </summary>
        /// <value>The <see cref="PersistenceConfiguration">persistence configuration</see>.</value>
        protected internal virtual PersistenceConfiguration Configuration { get; }

        /// <summary>
        /// Persists the specified commit.
        /// </summary>
        /// <param name="commit">The <see cref="Commit">commit</see> to append.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual async Task Persist( Commit commit, CancellationToken cancellationToken )
        {
            Arg.NotNull( commit, nameof( commit ) );

            using ( var connection = Configuration.CreateConnection() )
            {
                await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );

                using ( var transaction = connection.BeginTransaction() )
                {
                    using ( var command = Configuration.Events.NewSaveEventCommand() )
                    {
                        command.Connection = connection;
                        command.Transaction = transaction;
                        await AppendEvents( command, commit.Id, commit.Events, commit.Version, cancellationToken ).ConfigureAwait( false );
                    }

                    await TransitionState( transaction, commit.Saga, cancellationToken ).ConfigureAwait( false );

                    using ( var command = Configuration.Messages.NewEnqueueCommand() )
                    {
                        command.Connection = connection;
                        command.Transaction = transaction;
                        await EnqueueMessages( command, commit.Messages, cancellationToken ).ConfigureAwait( false );
                    }

                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Appends a set of events for the specified version using the provided command.
        /// </summary>
        /// <param name="command">The <see cref="DbCommand">command</see> used to save events.</param>
        /// <param name="aggregateId">The identifier of the aggregate to append the events to.</param>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> to commit.</param>
        /// <param name="version">The version of the aggregate the events are being saved for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        protected virtual async Task AppendEvents( DbCommand command, object aggregateId, IEnumerable<IEvent> events, int version, CancellationToken cancellationToken )
        {
            using ( var iterator = events.GetEnumerator() )
            {
                if ( !iterator.MoveNext() )
                {
                    return;
                }

                var sequence = 0;

                do
                {
                    var @event = iterator.Current;

                    @event.Version = version;
                    @event.Sequence = sequence++;

                    var eventDescriptor = new EventDescriptor<object>( aggregateId, @event );

                    try
                    {
                        await Configuration.Events.SaveEvent( command, eventDescriptor, cancellationToken ).ConfigureAwait( false );
                    }
                    catch ( DbException ex ) when ( ex.IsPrimaryKeyViolation() )
                    {
                        throw new ConcurrencyException( SR.AggregateVersionOutDated.FormatDefault( aggregateId, version ) );
                    }
                }
                while ( iterator.MoveNext() );
            }
        }

        /// <summary>
        /// Transitions the state of a saga.
        /// </summary>
        /// <param name="transaction">The current <see cref="DbTransaction">database transaction</see>.</param>
        /// <param name="saga">The <see cref="ISagaInstance">saga instance</see> to perform a state transition on.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        /// <remarks>If the <paramref name="saga"/> is <c>null</c>, no action is performed.</remarks>
        protected virtual async Task TransitionState( DbTransaction transaction, ISagaInstance saga, CancellationToken cancellationToken )
        {
            Arg.NotNull( transaction, nameof( transaction ) );

            if ( saga == null )
            {
                return;
            }

            var connection = transaction.Connection;

            if ( saga.Completed )
            {
                if ( !saga.IsNew )
                {
                    using ( var command = Configuration.Sagas.NewCompleteCommand( saga.Data ) )
                    {
                        command.Connection = connection;
                        command.Transaction = transaction;
                        await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );
                    }
                }

                saga.Complete();
            }
            else
            {
                using ( var stream = Configuration.Sagas.Serialize( saga.Data ) )
                using ( var command = Configuration.Sagas.NewStoreCommand( saga.Data, saga.CorrelationProperty, stream ) )
                {
                    command.Connection = connection;
                    command.Transaction = transaction;
                    await command.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );
                }

                saga.Update();
            }
        }

        /// <summary>
        /// Enqueues the specified messages using the provided command.
        /// </summary>
        /// <param name="command">The <see cref="DbCommand">command</see> used to enqueue messages.</param>
        /// <param name="messageDescriptors">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IMessageDescriptor">messages</see> to enqueue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        protected virtual async Task EnqueueMessages( DbCommand command, IEnumerable<IMessageDescriptor> messageDescriptors, CancellationToken cancellationToken )
        {
            // note: stagger queue times by 100ns (1 tick = 100ns) to ensure that items are properly ordered
            var clock = Configuration.Clock;
            var enqueueTime = clock.Now.UtcDateTime;
            var ticks = 0L;

            foreach ( var messageDescriptor in messageDescriptors )
            {
                var item = new SqlMessageQueueItem()
                {
                    EnqueueTime = enqueueTime,
                    DueTime = messageDescriptor.Options.GetDeliveryTime( clock ).UtcDateTime + FromTicks( ticks++ ),
                    MessageType = messageDescriptor.MessageType,
                    Revision = messageDescriptor.Message.Revision,
                    Message = Configuration.MessageSerializer.Serialize( messageDescriptor.Message ),
                };

                await Configuration.Messages.Enqueue( command, item, cancellationToken ).ConfigureAwait( false );
            }
        }

        /// <summary>
        /// Represents the configuration for persistence.
        /// </summary>
        protected internal class PersistenceConfiguration
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PersistenceConfiguration"/> class.
            /// </summary>
            /// <param name="messageQueueConfiguration">The <see cref="SqlMessageQueueConfiguration">configuration</see> for queued messages.</param>
            /// <param name="eventStoreConfiguration">The <see cref="SqlEventStoreConfiguration">configuration</see> for events.</param>
            /// <param name="sagaStorageConfiguration">The <see cref="SqlSagaStorageConfiguration">configuration</see> for sagas.</param>
            public PersistenceConfiguration(
                SqlMessageQueueConfiguration messageQueueConfiguration,
                SqlEventStoreConfiguration eventStoreConfiguration,
                SqlSagaStorageConfiguration sagaStorageConfiguration )
            {
                Arg.NotNull( messageQueueConfiguration, nameof( messageQueueConfiguration ) );
                Arg.NotNull( eventStoreConfiguration, nameof( eventStoreConfiguration ) );
                Arg.NotNull( sagaStorageConfiguration, nameof( sagaStorageConfiguration ) );

                Messages = messageQueueConfiguration;
                Events = eventStoreConfiguration;
                Sagas = sagaStorageConfiguration;
            }

            /// <summary>
            /// Create a new database connection.
            /// </summary>
            /// <returns>A new, configured <see cref="DbConnection">database connection</see>.</returns>
            /// <exception cref="InvalidOperationException">The underlying configurations do not all use the same database connection string.</exception>
            public virtual DbConnection CreateConnection()
            {
                Contract.Ensures( Contract.Result<DbConnection>() != null );

                var connection = Messages.CreateConnection();
                var comparer = StringComparer.OrdinalIgnoreCase;

                using ( var connection2 = Events.CreateConnection() )
                {
                    if ( comparer.Equals( connection.ConnectionString, connection2.ConnectionString ) )
                    {
                        using ( var connection3 = Sagas.CreateConnection() )
                        {
                            if ( comparer.Equals( connection.ConnectionString, connection3.ConnectionString ) )
                            {
                                return connection;
                            }
                        }
                    }
                }

                throw new InvalidOperationException( SR.InvalidPersistenceConfiguration );
            }

            /// <summary>
            /// Gets the configured clock.
            /// </summary>
            /// <value>The configured <see cref="IClock">clock</see>.</value>
            public IClock Clock => Messages.Clock;

            /// <summary>
            /// Gets the serializer used to serialize and deserialize messages.
            /// </summary>
            /// <value>The <see cref="ISqlMessageSerializer{TMessage}">serializer</see> used to serialize and deserialize messages.</value>
            public ISqlMessageSerializer<IMessage> MessageSerializer => Messages.MessageSerializer;

            /// <summary>
            /// Gets the current message queue configuration.
            /// </summary>
            /// <value>The <see cref="SqlMessageQueueConfiguration">configuration</see> for queued messages.</value>
            public SqlMessageQueueConfiguration Messages { get; }

            /// <summary>
            /// Gets the current event store configuration.
            /// </summary>
            /// <value>The <see cref="SqlEventStoreConfiguration">configuration</see> for events.</value>
            public SqlEventStoreConfiguration Events { get; }

            /// <summary>
            /// Gets the current saga configuration.
            /// </summary>
            /// <value>The <see cref="SqlSagaStorageConfiguration">configuration</see> for sagas.</value>
            public SqlSagaStorageConfiguration Sagas { get; }
        }
    }
}