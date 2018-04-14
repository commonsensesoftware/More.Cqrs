namespace More.Domain.Events
{
    using More.Domain.Messaging;
    using More.Domain.Persistence;
    using More.Domain.Sagas;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    class EventStoreOnlyPersistence : SqlPersistence
    {
        EventStoreOnlyPersistence(
            SqlMessageQueueConfiguration messageQueueConfiguration,
            SqlEventStoreConfiguration eventStoreConfiguration,
            SqlSagaStorageConfiguration sagaStorageConfiguration )
            : base( messageQueueConfiguration, eventStoreConfiguration, sagaStorageConfiguration ) { }

        internal static EventStoreOnlyPersistence New( SqlEventStoreConfiguration eventStoreConfiguration, DatabaseFixture database )
        {
            return new EventStoreOnlyPersistence(
                new SqlMessageQueueConfigurationBuilder().HasConnectionString( database.ConnectionString ).CreateConfiguration(),
                eventStoreConfiguration,
                new SqlSagaStorageConfigurationBuilder().HasConnectionString( database.ConnectionString ).CreateConfiguration() );
        }

        protected override Task EnqueueMessages( DbCommand command, IEnumerable<IMessageDescriptor> messageDescriptors, CancellationToken cancellationToken ) => CompletedTask;

        protected override Task TransitionState( DbTransaction transaction, ISagaInstance saga, CancellationToken cancellationToken ) => CompletedTask;
    }
}
