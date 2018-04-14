// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;

    /// <summary>
    /// Represents the base implementation for saga data.
    /// </summary>
    public abstract class SagaData : ISagaData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SagaData"/> class.
        /// </summary>
        protected SagaData() { }

        /// <summary>
        /// Gets or sets the saga identifier.
        /// </summary>
        /// <value>The unique saga identifier.</value>
        /// <remarks>This property is assigned by the infrastructure and should never been explicitly set.</remarks>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the version of the saga.
        /// </summary>
        /// <value>The saga version.</value>
        public int Version { get; set; }
    }
}