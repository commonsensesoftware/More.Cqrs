// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using More.Domain.Events;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an active saga instance.
    /// </summary>
    /// <typeparam name="TData">The type of saga data.</typeparam>
    public class SagaInstance<TData> : ISagaInstance where TData : class, ISagaData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SagaInstance{TData}"/> class.
        /// </summary>
        /// <param name="saga">The active <see cref="ISaga{TData}">saga</see>.</param>
        /// <param name="metadata">The associated <see cref="SagaMetadata">saga metadata</see>.</param>
        /// <param name="clock">The associated <see cref="IClock">clock</see>.</param>
        public SagaInstance( ISaga<TData> saga, SagaMetadata metadata, IClock clock )
        {
            Arg.NotNull( saga, nameof( saga ) );
            Arg.NotNull( metadata, nameof( metadata ) );
            Arg.NotNull( clock, nameof( clock ) );

            Instance = saga;
            Metadata = metadata;
            Modified = Created = clock.Now;
            Clock = clock;
        }

        /// <summary>
        /// Gets the associated clock.
        /// </summary>
        /// <value>The associated <see cref="IClock">clock</see>.</value>
        protected IClock Clock { get; }

        /// <summary>
        /// Gets the saga identifier.
        /// </summary>
        /// <value>The unique saga identifier.</value>
        public Guid SagaId { get; private set; }

        /// <summary>
        /// Gets the saga data.
        /// </summary>
        /// <value>The associated <see cref="ISagaData">saga data</see>.</value>
        public TData Data => Instance.Data;

        ISagaData ISagaInstance.Data => Instance.Data;

        /// <summary>
        /// Gets the active saga.
        /// </summary>
        /// <value>The active <see cref="ISaga{TData}">saga</see>.</value>
        public ISaga<TData> Instance { get; }

        /// <summary>
        /// Gets the metadata for the saga.
        /// </summary>
        /// <value>The <see cref="SagaMetadata">saga's metadata</see>.</value>
        public SagaMetadata Metadata { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the activate saga instance is new.
        /// </summary>
        /// <value>True if the saga instance is newly activated; otherwise, false.</value>
        public bool IsNew { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the saga is complete.
        /// </summary>
        /// <value>True if the saga is complete; otherwise, false.</value>
        public bool Completed => Instance.Completed;

        /// <summary>
        /// Gets or sets a value indicating whether the active saga was not found.
        /// </summary>
        /// <value>True if an active saga was not found; otherwise, false.</value>
        public bool NotFound { get; protected set; }

        /// <summary>
        /// Gets the date and time when the saga was created.
        /// </summary>
        /// <value>The <see cref="DateTimeOffset">date and time</see> when the saga was created.</value>
        public DateTimeOffset Created { get; }

        /// <summary>
        /// Gets or sets the date and time when the saga was last modified.
        /// </summary>
        /// <value>The <see cref="DateTimeOffset">date and time</see> when the saga was last modified.</value>
        public DateTimeOffset Modified { get; protected set; }

        /// <summary>
        /// Gets or sets proeprty used to correlate the saga.
        /// </summary>
        /// <value>The <see cref="CorrelationProperty">correlation property</see> used to correlate the saga, if any.</value>
        public CorrelationProperty CorrelationProperty { get; protected set; }

        /// <summary>
        /// Gets a read-only list of events recorded by the saga that have yet to be committed to storage.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of uncommitted <see cref="IEvent">events</see>.</value>
        public IReadOnlyList<IEvent> UncommittedEvents => Instance.UncommittedEvents;

        /// <summary>
        /// Accepts the changes made to the saga.
        /// </summary>
        public void AcceptChanges() => Instance.AcceptChanges();

        void ISagaInstance.AttachNew( ISagaData data ) => AttachNew( (TData) data );

        void ISagaInstance.AttachExisting( ISagaData data ) => AttachExisting( (TData) data );

        /// <summary>
        /// Attaches new saga data to the saga instance.
        /// </summary>
        /// <param name="data">The <typeparamref name="TData">saga data</typeparamref> to attach.</param>
        public virtual void AttachNew( TData data )
        {
            Arg.NotNull( data, nameof( data ) );

            IsNew = true;
            AttachExisting( data );
        }

        /// <summary>
        /// Attaches existing saga data to the saga instance.
        /// </summary>
        /// <param name="data">The <typeparamref name="TData">saga data</typeparamref> to attach.</param>
        public virtual void AttachExisting( TData data )
        {
            Arg.NotNull( data, nameof( data ) );

            SagaId = data.Id;
            Modified = Clock.Now;
            Instance.Data = data;

            var property = Metadata.CorrelationProperty;
            var value = property.GetValue( data );
            var defaultValue = property.PropertyType.DefaultValue();
            var isDefaultValue = !Equals( value, defaultValue );

            CorrelationProperty = new CorrelationProperty( property, value, isDefaultValue );
        }

        /// <summary>
        /// Marks the the active saga instance as completed.
        /// </summary>
        public virtual void Complete() => Modified = Clock.Now;

        /// <summary>
        /// Marks the the active saga instance as updated.
        /// </summary>
        public virtual void Update() => Modified = Clock.Now;

        /// <summary>
        /// Marks the active saga instance as not found.
        /// </summary>
        public virtual void MarkAsNotFound() => NotFound = true;

        /// <summary>
        /// Validates the changes made to the active saga instance.
        /// </summary>
        public virtual void ValidateChanges()
        {
            EnsureSagaIdIsUnmodified();

            if ( CorrelationProperty == null )
            {
                return;
            }

            if ( IsNew )
            {
                EnsureCorrelationPropertyHasValue();
            }

            EnsureCorrelationPropertyIsUnmodified();
        }

        void EnsureSagaIdIsUnmodified()
        {
            if ( Instance.Data.Id != SagaId )
            {
                throw new SagaException( SR.SagaIdModified.FormatDefault( Metadata.SagaType.Name ) );
            }
        }

        void EnsureCorrelationPropertyHasValue()
        {
            var property = CorrelationProperty.Property;
            var defaultValue = property.PropertyType.DefaultValue();

            if ( Equals( CorrelationProperty.Value, defaultValue ) )
            {
                throw new SagaException( SR.SagaCorrelationPropertyUnassigned.FormatDefault( property.Name, Metadata.SagaType.Name ) );
            }
        }

        void EnsureCorrelationPropertyIsUnmodified()
        {
            if ( !CorrelationProperty.IsDefaultValue )
            {
                return;
            }

            var property = CorrelationProperty.Property;
            var currentValue = property.GetValue( Instance.Data );
            var value = CorrelationProperty.Value;

            if ( !Equals( value, currentValue ) )
            {
                throw new SagaException( SR.SagaCorrelationPropertyModified.FormatDefault( property.Name, Metadata.SagaType.Name, value, currentValue ) );
            }
        }
    }
}