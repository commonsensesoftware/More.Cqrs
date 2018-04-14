namespace Contoso.Services.Composition
{
    using More.Domain.Events;
    using More.Domain.Messaging;
    using More.Domain.Persistence;
    using More.Domain.Sagas;
    using System;
    using System.Collections.Generic;
    using System.Composition.Hosting.Core;
    using System.Linq;
    using static System.Composition.Hosting.Core.CompositionOperation;
    using static System.Composition.Hosting.Core.ExportDescriptor;

    sealed class SqlConfigurationExportDescriptorProvider : ExportDescriptorProvider
    {
        readonly string connectionStringKey;
        readonly string subscriptionKey;
        readonly Type SubscriptionId = typeof( Guid );
        readonly Type Persistence = typeof( SqlPersistence );
        readonly Type IPersistence = typeof( IPersistence );
        readonly Type MessageQueueConfiguration = typeof( SqlMessageQueueConfiguration );
        readonly Type EventStoreConfiguration = typeof( SqlEventStoreConfiguration );
        readonly Type SagaStorageConfiguration = typeof( SqlSagaStorageConfiguration );

        internal SqlConfigurationExportDescriptorProvider( string connectionStringKey, string subscriptionKey )
        {
            this.connectionStringKey = connectionStringKey;
            this.subscriptionKey = subscriptionKey;
        }

        public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
        {
            if ( contract.ContractType == SubscriptionId )
            {
                // [Export( "key" )]
                // public struct Guid( [Setting( "key" )] string s ) { }
                return ExportSubscriptionIdConfigurationPart( contract, descriptorAccessor );
            }
            else if ( contract.ContractType == Persistence || contract.ContractType == IPersistence )
            {
                // [Export( nameof( TAggregate ), typeof( SqlPersistence ) )]
                // [Export( nameof( TAggregate ), typeof( IPersistence ) )]
                // public class SqlPersistence( SqlMessageQueueConfiguration messageQueueConfiguration,
                //                              [Import( nameof( TAggregate ) )]  SqlEventStoreConfiguration eventStoreConfiguration,
                //                              SqlSagaStorageConfiguration sagaStorageConfiguration ) { }
                return ExportPersistencePart( contract, descriptorAccessor );
            }
            else if ( contract.ContractType == MessageQueueConfiguration )
            {
                // [Export]
                // public class SqlMessageQueueConfiguration( [Setting( "key" )] string connectionString ) { }
                return ExportMessageQueueConfigurationPart( contract, descriptorAccessor );
            }
            else if ( contract.ContractType == SagaStorageConfiguration )
            {
                // [Export]
                // public class SqlSagaStorageConfiguration( [Setting( "key" )] string connectionString ) { }
                return ExportSagaStorageConfigurationPart( contract, descriptorAccessor );
            }
            else if ( contract.ContractType == EventStoreConfiguration )
            {
                // [Export( nameof( TAggregate ) )]
                // public class SqlEventStoreConfiguration( [Setting( "key" )] string connectionString, string entityName ) { }
                return ExportEventStoreConfigurationPart( contract, descriptorAccessor );
            }

            return NoExportDescriptors;
        }

        IEnumerable<ExportDescriptorPromise> ExportSubscriptionIdConfigurationPart( CompositionContract contract, DependencyAccessor descriptorAccessor )
        {
            const bool shared = true;
            var subscriptionId = new StringDependencyResolver( descriptorAccessor, subscriptionKey );

            yield return new ExportDescriptorPromise(
               contract,
               nameof( SqlConfigurationExportDescriptorProvider ),
               shared,
               subscriptionId.Resolve,
               d => Create( ( c, o ) => SubscriptionIdActivator( c, o, d ), NoMetadata ) );
        }

        IEnumerable<ExportDescriptorPromise> ExportPersistencePart( CompositionContract contract, DependencyAccessor descriptorAccessor )
        {
            const bool shared = true;
            var entityName = contract.ContractName;
            var persistenceResolver = new PersistenceDependencyResolver( descriptorAccessor, entityName );

            yield return new ExportDescriptorPromise(
               contract,
               nameof( SqlConfigurationExportDescriptorProvider ),
               shared,
               persistenceResolver.Resolve,
               d => Create( ( c, o ) => PersistenceActivator( c, o, d ), NoMetadata ) );
        }

        IEnumerable<ExportDescriptorPromise> ExportMessageQueueConfigurationPart( CompositionContract contract, DependencyAccessor descriptorAccessor )
        {
            const bool shared = true;
            var connectionString = new StringDependencyResolver( descriptorAccessor, connectionStringKey );

            yield return new ExportDescriptorPromise(
               contract,
               nameof( SqlConfigurationExportDescriptorProvider ),
               shared,
               connectionString.Resolve,
               d => Create( ( c, o ) => MessageQueueConfigurationActivator( c, o, d ), NoMetadata ) );
        }

        IEnumerable<ExportDescriptorPromise> ExportSagaStorageConfigurationPart( CompositionContract contract, DependencyAccessor descriptorAccessor )
        {
            const bool shared = true;
            var connectionString = new StringDependencyResolver( descriptorAccessor, connectionStringKey );

            yield return new ExportDescriptorPromise(
                contract,
                nameof( SqlConfigurationExportDescriptorProvider ),
                shared,
                connectionString.Resolve,
                d => Create( ( c, o ) => SagaStorageConfigurationActivator( c, o, d ), NoMetadata ) );
        }

        IEnumerable<ExportDescriptorPromise> ExportEventStoreConfigurationPart( CompositionContract contract, DependencyAccessor descriptorAccessor )
        {
            const bool shared = true;
            var connectionString = new StringDependencyResolver( descriptorAccessor, connectionStringKey );
            var entityName = contract.ContractName;

            yield return new ExportDescriptorPromise(
                contract,
                nameof( SqlConfigurationExportDescriptorProvider ),
                shared,
                connectionString.Resolve,
                d => Create( ( c, o ) => EventStoreConfigurationActivator( c, o, d, entityName ), NoMetadata ) );
        }

        static object SubscriptionIdActivator( LifetimeContext context, CompositionOperation operation, IEnumerable<CompositionDependency> dependencies )
        {
            var dependency = dependencies.Single().Target.GetDescriptor();
            var subscriptionId = (string) Run( context, dependency.Activator );

            return new Guid( subscriptionId );
        }

        static object PersistenceActivator( LifetimeContext context, CompositionOperation operation, IEnumerable<CompositionDependency> dependencies )
        {
            var map = dependencies.ToDictionary( d => d.Contract.ContractType, d => d.Target.GetDescriptor().Activator );
            var queueConfiguration = (SqlMessageQueueConfiguration) Run( context, map[typeof( SqlMessageQueueConfiguration )] );
            var eventStoreConfiguration = (SqlEventStoreConfiguration) Run( context, map[typeof( SqlEventStoreConfiguration )] );
            var sagaConfiguration = (SqlSagaStorageConfiguration) Run( context, map[typeof( SqlSagaStorageConfiguration )] );

            return new SqlPersistence( queueConfiguration, eventStoreConfiguration, sagaConfiguration );
        }

        static object MessageQueueConfigurationActivator( LifetimeContext context, CompositionOperation operation, IEnumerable<CompositionDependency> dependencies )
        {
            var dependency = dependencies.Single().Target.GetDescriptor();
            var connectionString = (string) Run( context, dependency.Activator );
            var builder = new SqlMessageQueueConfigurationBuilder();

            builder.HasConnectionString( connectionString );

            return builder.CreateConfiguration();
        }

        static object SagaStorageConfigurationActivator( LifetimeContext context, CompositionOperation operation, IEnumerable<CompositionDependency> dependencies )
        {
            var dependency = dependencies.Single().Target.GetDescriptor();
            var connectionString = (string) Run( context, dependency.Activator );
            var builder = new SqlSagaStorageConfigurationBuilder();

            builder.HasConnectionString( connectionString );

            return builder.CreateConfiguration();
        }

        static object EventStoreConfigurationActivator( LifetimeContext context, CompositionOperation operation, IEnumerable<CompositionDependency> dependencies, string entityName )
        {
            var dependency = dependencies.Single().Target.GetDescriptor();
            var connectionString = (string) Run( context, dependency.Activator );
            var builder = new SqlEventStoreConfigurationBuilder();

            builder.HasConnectionString( connectionString );

            if ( !string.IsNullOrEmpty( entityName ) )
            {
                builder.HasEntityName( entityName );
            }

            return builder.CreateConfiguration();
        }

        sealed class StringDependencyResolver
        {
            readonly DependencyAccessor descriptorAccessor;
            readonly string key;

            internal StringDependencyResolver( DependencyAccessor descriptorAccessor, string key )
            {
                this.descriptorAccessor = descriptorAccessor;
                this.key = key;
            }

            internal IEnumerable<CompositionDependency> Resolve()
            {
                const string site = "configuration";
                const bool isPrerequisite = true;
                var metadata = new Dictionary<string, object>() { ["Key"] = key };
                var contract = new CompositionContract( typeof( string ), key, metadata );

                return descriptorAccessor.ResolveDependencies( site, contract, isPrerequisite );
            }
        }

        sealed class PersistenceDependencyResolver
        {
            readonly DependencyAccessor descriptorAccessor;
            readonly string entityName;

            internal PersistenceDependencyResolver( DependencyAccessor descriptorAccessor, string entityName )
            {
                this.descriptorAccessor = descriptorAccessor;
                this.entityName = entityName;
            }

            internal IEnumerable<CompositionDependency> Resolve()
            {
                const string site = "configuration";
                const bool isPrerequisite = true;
                var queueContract = new CompositionContract( typeof( SqlMessageQueueConfiguration ) );
                var eventStoreContract = new CompositionContract( typeof( SqlEventStoreConfiguration ), entityName );
                var sagaContract = new CompositionContract( typeof( SqlSagaStorageConfiguration ) );

                yield return descriptorAccessor.ResolveRequiredDependency( site, queueContract, isPrerequisite );
                yield return descriptorAccessor.ResolveRequiredDependency( site, eventStoreContract, isPrerequisite );
                yield return descriptorAccessor.ResolveRequiredDependency( site, sagaContract, isPrerequisite );
            }
        }
    }
}