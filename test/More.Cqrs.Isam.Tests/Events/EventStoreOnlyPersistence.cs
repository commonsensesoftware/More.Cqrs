namespace More.Domain.Events
{
    using Microsoft.Database.Isam;
    using More.Domain.Messaging;
    using More.Domain.Persistence;
    using More.Domain.Sagas;
    using System.Collections.Generic;
    using System.Threading;

    class EventStoreOnlyPersistence : IsamPersistence
    {
        EventStoreOnlyPersistence(
            IsamMessageQueueConfiguration messageQueueConfiguration,
            IsamEventStoreConfiguration eventStoreConfiguration,
            IsamSagaStorageConfiguration sagaStorageConfiguration )
            : base( messageQueueConfiguration, eventStoreConfiguration, sagaStorageConfiguration ) { }

        internal static EventStoreOnlyPersistence New( IsamEventStoreConfiguration eventStoreConfiguration, DatabaseFixture database )
        {
            var connectionString = database.ConnectionString;
            var messageQueueConfiguration = new IsamMessageQueueConfigurationBuilder().HasConnectionString( connectionString ).CreateConfiguration();
            var sagaStorageConfiguration = new IsamSagaStorageConfigurationBuilder().HasConnectionString( connectionString ).CreateConfiguration();

            return new EventStoreOnlyPersistence( messageQueueConfiguration, eventStoreConfiguration, sagaStorageConfiguration );
        }

        protected override void EnqueueMessages( IsamDatabase database, IEnumerable<IMessageDescriptor> messageDescriptors, CancellationToken cancellationToken ) { }

        protected override void TransitionState( IsamDatabase database, ISagaInstance saga ) { }
    }
}