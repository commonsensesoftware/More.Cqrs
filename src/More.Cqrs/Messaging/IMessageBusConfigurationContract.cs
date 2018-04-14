// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain;
    using More.Domain.Commands;
    using More.Domain.Events;
    using More.Domain.Persistence;
    using System;
    using System.Diagnostics.Contracts;

    [ContractClassFor( typeof( IMessageBusConfiguration ) )]
    abstract class IMessageBusConfigurationContract : IMessageBusConfiguration
    {
        IClock IMessageBusConfiguration.Clock
        {
            get
            {
                Contract.Ensures( Contract.Result<IClock>() != null );
                return null;
            }
        }

        IMapPersistence IMessageBusConfiguration.Persistence
        {
            get
            {
                Contract.Ensures( Contract.Result<IMapPersistence>() != null );
                return null;
            }
        }

        IMessageSender IMessageBusConfiguration.MessageSender
        {
            get
            {
                Contract.Ensures( Contract.Result<IMessageSender>() != null );
                return null;
            }
        }

        IMessageReceiver IMessageBusConfiguration.MessageReceiver
        {
            get
            {
                Contract.Ensures( Contract.Result<IMessageReceiver>() != null );
                return null;
            }
        }

        IEventReceiverRegistrar IMessageBusConfiguration.EventReceivers
        {
            get
            {
                Contract.Ensures( Contract.Result<IEventReceiverRegistrar>() != null );
                return null;
            }
        }

        ICommandHandlerRegistrar IMessageBusConfiguration.CommandHandlers
        {
            get
            {
                Contract.Ensures( Contract.Result<ICommandHandlerRegistrar>() != null );
                return null;
            }
        }

        SagaConfiguration IMessageBusConfiguration.Sagas
        {
            get
            {
                Contract.Ensures( Contract.Result<SagaConfiguration>() != null );
                return null;
            }
        }

        IUniqueIdGenerator IMessageBusConfiguration.UniqueIdGenerator
        {
            get
            {
                Contract.Ensures( Contract.Result<IUniqueIdGenerator>() != null );
                return null;
            }
        }

        object IServiceProvider.GetService( Type serviceType ) => null;
    }
}