// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using More.Domain.Events;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines the behavior of an active saga instance.
    /// </summary>
    [ContractClass( typeof( ISagaInstanceContract ) )]
    public interface ISagaInstance
    {
        /// <summary>
        /// Gets the saga identifier.
        /// </summary>
        /// <value>The unique saga identifier.</value>
        Guid SagaId { get; }

        /// <summary>
        /// Gets the saga data.
        /// </summary>
        /// <value>The associated <see cref="ISagaData">saga data</see>.</value>
        ISagaData Data { get; }

        /// <summary>
        /// Gets the metadata for the saga.
        /// </summary>
        /// <value>The <see cref="SagaMetadata">saga's metadata</see>.</value>
        SagaMetadata Metadata { get; }

        /// <summary>
        /// Gets a value indicating whether the activate saga instance is new.
        /// </summary>
        /// <value>True if the saga instance is newly activated; otherwise, false.</value>
        bool IsNew { get; }

        /// <summary>
        /// Gets a value indicating whether the saga is complete.
        /// </summary>
        /// <value>True if the saga is complete; otherwise, false.</value>
        bool Completed { get; }

        /// <summary>
        /// Gets a value indicating whether the active saga was not found.
        /// </summary>
        /// <value>True if an active saga was not found; otherwise, false.</value>
        bool NotFound { get; }

        /// <summary>
        /// Gets the date and time when the saga was created.
        /// </summary>
        /// <value>The <see cref="DateTimeOffset">date and time</see> when the saga was created.</value>
        DateTimeOffset Created { get; }

        /// <summary>
        /// Gets the date and time when the saga was last modified.
        /// </summary>
        /// <value>The <see cref="DateTimeOffset">date and time</see> when the saga was last modified.</value>
        DateTimeOffset Modified { get; }

        /// <summary>
        /// Gets proeprty used to correlate the saga.
        /// </summary>
        /// <value>The <see cref="CorrelationProperty">correlation property</see> used to correlate the saga, if any.</value>
        CorrelationProperty CorrelationProperty { get; }

        /// <summary>
        /// Gets a read-only list of events recorded by the saga that have yet to be committed to storage.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of uncommitted <see cref="IEvent">events</see>.</value>
        IReadOnlyList<IEvent> UncommittedEvents { get; }

        /// <summary>
        /// Accepts the changes made to the saga.
        /// </summary>
        void AcceptChanges();

        /// <summary>
        /// Attaches new saga data to the saga instance.
        /// </summary>
        /// <param name="data">The <see cref="ISagaData">saga data</see> to attach.</param>
        void AttachNew( ISagaData data );

        /// <summary>
        /// Attaches existing saga data to the saga instance.
        /// </summary>
        /// <param name="data">The <see cref="ISagaData">saga data</see> to attach.</param>
        void AttachExisting( ISagaData data );

        /// <summary>
        /// Marks the the active saga instance as completed.
        /// </summary>
        void Complete();

        /// <summary>
        /// Marks the the active saga instance as updated.
        /// </summary>
        void Update();

        /// <summary>
        /// Marks the active saga instance as not found.
        /// </summary>
        void MarkAsNotFound();

        /// <summary>
        /// Validates the changes made to the active saga instance.
        /// </summary>
        void ValidateChanges();
    }
}