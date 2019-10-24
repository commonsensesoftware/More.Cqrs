// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain.Sagas;
    using System;

    /// <summary>
    /// Represents the configuration for sagas.
    /// </summary>
    public class SagaConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SagaConfiguration"/> class.
        /// </summary>
        /// <param name="storage">The configure <see cref="IStoreSagaData">saga storage</see>.</param>
        /// <param name="metadata">The configured <see cref="SagaMetadataCollection">collection saga metadata</see>.</param>
        public SagaConfiguration( IStoreSagaData storage, SagaMetadataCollection metadata )
        {
            Storage = storage;
            Metadata = metadata;
        }

        /// <summary>
        /// Gets the configure saga storage.
        /// </summary>
        /// <value>The configure <see cref="IStoreSagaData">saga storage</see>.</value>
        public IStoreSagaData Storage { get; }

        /// <summary>
        /// Gets the configured saga metadata.
        /// </summary>
        /// <value>The configured <see cref="SagaMetadataCollection">collection saga metadata</see>.</value>
        public SagaMetadataCollection Metadata { get; }
    }
}