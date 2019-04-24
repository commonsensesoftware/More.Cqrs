// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain.Sagas;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    static class DefaultConfiguration
    {
        internal static IMessageSender MessageSender { get; } = new UnconfiguredMessageSender();

        internal static IMessageReceiver MessageReceiver { get; } = new UnconfiguredMessageReceiver();

        internal static IStoreSagaData SagaStorage { get; } = new UnconfiguredSagaStorage();

        sealed class UnconfiguredMessageSender : IMessageSender
        {
            public Task Send( IEnumerable<IMessageDescriptor> messages, CancellationToken cancellationToken ) =>
                throw new NotSupportedException( SR.NoConfiguredMessageSender );
        }

        sealed class UnconfiguredMessageReceiver : IMessageReceiver
        {
            public IDisposable Subscribe( IObserver<IMessageDescriptor> observer ) => throw new NotSupportedException( SR.NoConfiguredMessageReceiver );
        }

        sealed class UnconfiguredSagaStorage : IStoreSagaData
        {
            public Task Store( ISagaData data, CorrelationProperty correlationProperty, CancellationToken cancellationToken ) =>
                throw new NotSupportedException( SR.NoConfiguredSagaStorage );

            public Task<TData> Retrieve<TData>( Guid sagaId, CancellationToken cancellationToken ) where TData : class, ISagaData =>
                throw new NotSupportedException( SR.NoConfiguredSagaStorage );

            public Task<TData> Retrieve<TData>( string propertyName, object propertyValue, CancellationToken cancellationToken ) where TData : class, ISagaData =>
                throw new NotSupportedException( SR.NoConfiguredSagaStorage );

            public Task Complete( ISagaData data, CancellationToken cancellationToken ) => throw new NotSupportedException( SR.NoConfiguredSagaStorage );
        }
    }
}