// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Persistence
{
    using Microsoft.Database.Isam;
    using Microsoft.Isam.Esent.Interop;
    using More.Domain.Events;
    using More.Domain.Messaging;
    using More.Domain.Options;
    using More.Domain.Sagas;
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;
    using static System.TimeSpan;

    /// <summary>
    /// Represents persistence backed by an ISAM database.
    /// </summary>
    public class IsamPersistence : IPersistence
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsamPersistence"/> class.
        /// </summary>
        /// <param name="messageQueueConfiguration">The <see cref="IsamMessageQueueConfiguration">configuration</see> for queued messages.</param>
        /// <param name="eventStoreConfiguration">The <see cref="IsamEventStoreConfiguration">configuration</see> for events.</param>
        /// <param name="sagaStorageConfiguration">The <see cref="IsamSagaStorageConfiguration">configuration</see> for sagas.</param>
        public IsamPersistence(
            IsamMessageQueueConfiguration messageQueueConfiguration,
            IsamEventStoreConfiguration eventStoreConfiguration,
            IsamSagaStorageConfiguration sagaStorageConfiguration )
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
        public virtual Task Persist( Commit commit, CancellationToken cancellationToken )
        {
            Arg.NotNull( commit, nameof( commit ) );

            var connection = Configuration.CreateConnection();

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                using ( var transaction = new IsamTransaction( session ) )
                {
                    AppendEvents( database, commit.Id, commit.Events, commit.Version, cancellationToken );
                    TransitionState( database, commit.Saga );
                    EnqueueMessages( database, commit.Messages, cancellationToken );

                    if ( cancellationToken.IsCancellationRequested )
                    {
                        transaction.Rollback();
                    }
                    else
                    {
                        transaction.Commit();
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            return CompletedTask;
        }

        /// <summary>
        /// Appends a set of events for the specified version using the provided command.
        /// </summary>
        /// <param name="database">The <see cref="DbCommand">command</see> used to save events.</param>
        /// <param name="aggregateId">The identifier of the aggregate to append the events to.</param>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> to commit.</param>
        /// <param name="version">The version of the aggregate the events are being saved for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        protected virtual void AppendEvents( IsamDatabase database, object aggregateId, IEnumerable<IEvent> events, int version, CancellationToken cancellationToken )
        {
            using ( var iterator = events.GetEnumerator() )
            {
                if ( !iterator.MoveNext() )
                {
                    return;
                }

                var store = Configuration.Events;
                var sequence = 0;

                using ( var cursor = database.OpenCursor( store.TableName ) )
                {
                    do
                    {
                        var @event = iterator.Current;

                        @event.Version = version;
                        @event.Sequence = sequence++;

                        var eventDescriptor = new EventDescriptor<object>( aggregateId, @event );

                        try
                        {
                            store.SaveEvent( cursor, eventDescriptor );
                        }
                        catch ( EsentKeyDuplicateException )
                        {
                            throw new ConcurrencyException( SR.AggregateVersionOutDated.FormatDefault( aggregateId, version ) );
                        }
                    }
                    while ( !cancellationToken.IsCancellationRequested && iterator.MoveNext() );
                }
            }
        }

        /// <summary>
        /// Transitions the state of a saga.
        /// </summary>
        /// <param name="database">The current <see cref="IsamDatabase">database</see>.</param>
        /// <param name="saga">The <see cref="ISagaInstance">saga instance</see> to perform a state transition on.</param>
        /// <remarks>If the <paramref name="saga"/> is <c>null</c>, no action is performed.</remarks>
        protected virtual void TransitionState( IsamDatabase database, ISagaInstance saga )
        {
            Arg.NotNull( database, nameof( database ) );

            if ( saga == null )
            {
                return;
            }

            var store = Configuration.Sagas;

            using ( var cursor = database.OpenCursor( store.TableName ) )
            {
                if ( saga.Completed )
                {
                    if ( !saga.IsNew )
                    {
                        store.Complete( cursor, saga.Data );
                    }

                    saga.Complete();
                }
                else
                {
                    store.Store( cursor, saga.Data, saga.CorrelationProperty );
                    saga.Update();
                }
            }
        }

        /// <summary>
        /// Enqueues the specified messages using the provided database.
        /// </summary>
        /// <param name="database">The <see cref="IsamDatabase">database</see> used to enqueue messages.</param>
        /// <param name="messageDescriptors">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IMessageDescriptor">messages</see> to enqueue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        protected virtual void EnqueueMessages( IsamDatabase database, IEnumerable<IMessageDescriptor> messageDescriptors, CancellationToken cancellationToken )
        {
            // note: stagger queue times by 100ns (1 tick = 100ns) to ensure that items are properly ordered
            var clock = Configuration.Clock;
            var enqueueTime = clock.Now.UtcDateTime;
            var ticks = 0L;

            foreach ( var messageDescriptor in messageDescriptors )
            {
                var item = new IsamMessageQueueItem()
                {
                    EnqueueTime = enqueueTime,
                    DueTime = messageDescriptor.Options.GetDeliveryTime( clock ).UtcDateTime + FromTicks( ticks++ ),
                    MessageType = messageDescriptor.MessageType,
                    Revision = messageDescriptor.Message.Revision,
                    Message = Configuration.MessageSerializer.Serialize( messageDescriptor.Message ),
                };

                Configuration.Messages.Enqueue( database, item );

                if ( cancellationToken.IsCancellationRequested )
                {
                    break;
                }
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
            /// <param name="messageQueueConfiguration">The <see cref="IsamMessageQueueConfiguration">configuration</see> for queued messages.</param>
            /// <param name="eventStoreConfiguration">The <see cref="IsamEventStoreConfiguration">configuration</see> for events.</param>
            /// <param name="sagaStorageConfiguration">The <see cref="IsamSagaStorageConfiguration">configuration</see> for sagas.</param>
            public PersistenceConfiguration(
                IsamMessageQueueConfiguration messageQueueConfiguration,
                IsamEventStoreConfiguration eventStoreConfiguration,
                IsamSagaStorageConfiguration sagaStorageConfiguration )
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
            /// <returns>A new, configured <see cref="IsamConnection">database connection</see>.</returns>
            /// <exception cref="InvalidOperationException">The underlying configurations do not all use the same database connection string.</exception>
            public virtual IsamConnection CreateConnection()
            {
                var connection1 = Events.CreateConnection();
                var connection2 = Messages.CreateConnection();
                var comparer = StringComparer.OrdinalIgnoreCase;

                if ( comparer.Equals( connection1.DatabaseName, connection2.DatabaseName ) )
                {
                    var connection3 = Sagas.CreateConnection();

                    if ( comparer.Equals( connection1.DatabaseName, connection3.DatabaseName ) )
                    {
                        return connection1;
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
            /// <value>The <see cref="IIsamMessageSerializer{TMessage}">serializer</see> used to serialize and deserialize messages.</value>
            public IIsamMessageSerializer<IMessage> MessageSerializer => Messages.MessageSerializer;

            /// <summary>
            /// Gets the current message queue configuration.
            /// </summary>
            /// <value>The <see cref="IsamMessageQueueConfiguration">configuration</see> for queued messages.</value>
            public IsamMessageQueueConfiguration Messages { get; }

            /// <summary>
            /// Gets the current event store configuration.
            /// </summary>
            /// <value>The <see cref="IsamEventStoreConfiguration">configuration</see> for events.</value>
            public IsamEventStoreConfiguration Events { get; }

            /// <summary>
            /// Gets the current saga configuration.
            /// </summary>
            /// <value>The <see cref="IsamSagaStorageConfiguration">configuration</see> for sagas.</value>
            public IsamSagaStorageConfiguration Sagas { get; }
        }
    }
}