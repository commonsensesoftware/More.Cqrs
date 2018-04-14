namespace Contoso.Services.Composition.Conventions
{
    using Contoso.Services.Components;
    using More.ComponentModel;
    using More.Domain;
    using More.Domain.Commands;
    using More.Domain.Events;
    using More.Domain.Messaging;
    using More.Domain.Persistence;
    using More.Domain.Sagas;
    using System;
    using System.Composition.Convention;
    using System.Reflection;
    using static DefaultConfigurationSettings;

    sealed class MessageBusConvention : IRule<ConventionContext>
    {
        public void Evaluate( ConventionContext context )
        {
            var conventions = context.Conventions;
            var messageReceiverConstructorSelector = new ConstructorSelectionRule( typeof( Guid ), typeof( SqlMessageQueueConfiguration ) );
            var messageSenderConstructorSelector = new ConstructorSelectionRule( typeof( SqlMessageQueueConfiguration ) );
            var sagaStorageConstructorSelector = new ConstructorSelectionRule( typeof( SqlSagaStorageConfiguration ) );

            conventions.ForType<SystemClock>().Export<IClock>().Shared();
            conventions.ForType<SequentialUniqueIdGenerator>().Export<IUniqueIdGenerator>().Shared();
            conventions.ForType<RuntimePersistenceMapper>().Export<IMapPersistence>().Shared();
            conventions.ForType<CommandRegistrar>().Export<ICommandHandlerRegistrar>().Shared();
            conventions.ForType<EventRegistrar>().Export<IEventReceiverRegistrar>().Shared();
            conventions.ForType<SqlMessageReceiver>()
                       .Export<IMessageReceiver>()
                       .SelectConstructor( messageReceiverConstructorSelector.Evaluate, OnImportMessageReceiverParameter )
                       .Shared();
            conventions.ForType<SqlMessageSender>()
                       .Export<IMessageSender>()
                       .SelectConstructor( messageSenderConstructorSelector.Evaluate )
                       .Shared();
            conventions.ForType<SqlSagaStorage>()
                       .Export<IStoreSagaData>()
                       .SelectConstructor( sagaStorageConstructorSelector.Evaluate )
                       .Shared();
            conventions.ForType<SagaConfiguration>().Export<SagaConfiguration>().Shared();
            conventions.ForType<MessageBusConfiguration>().Export<IMessageBusConfiguration>().Shared();
            conventions.ForType<MessageBus>().Export().ExportInterfaces().Shared();
        }

        static void OnImportMessageReceiverParameter( ParameterInfo parameter, ImportConventionBuilder import )
        {
            if ( parameter.ParameterType == typeof( Guid ) )
            {
                // [Import( "key" )] Guid subscriptionId
                import.AsContractName( SubscriptionKey );
            }
        }
    }
}