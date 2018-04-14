// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using Events;
    using Reflection;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using static Reflection.ActionDispatcherFactory<Events.IEvent>;

    /// <summary>
    /// Represents the base implementation of an aggregate.
    /// </summary>
    /// <typeparam name="TKey">The type of key for the aggregate.</typeparam>
    [DebuggerDisplay( "{GetType().Name}, Version = {Version}, Id = {Id}" )]
    public abstract class Aggregate<TKey> : IAggregate<TKey>
    {
        IActionDispatcher<IEvent> dispatcher;
        List<IEvent> events;

        /// <summary>
        /// Initializes a new instance of the <see cref="Aggregate{TKey}"/> class.
        /// </summary>
        protected Aggregate() { }

        IActionDispatcher<IEvent> Dispatcher => dispatcher ?? ( dispatcher = NewDispatcher( this ) );

        List<IEvent> Events => events ?? ( events = new List<IEvent>() );

        /// <summary>
        /// Gets or sets the aggregate identifier.
        /// </summary>
        /// <value>The unique aggregate identifier.</value>
        public virtual TKey Id { get; protected set; }

        /// <summary>
        /// Gets or sets the aggregate version.
        /// </summary>
        /// <value>The version of the aggregate.</value>
        /// <remarks>The version of the aggregate can be used for concurrency checks.</remarks>
        public virtual int Version { get; protected set; }

        /// <summary>
        /// Gets a read-only list of events recorded by the aggregate that have yet to be committed to storage.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of uncommitted <see cref="IEvent">events</see>.</value>
        public IReadOnlyList<IEvent> UncommittedEvents => Events;

        /// <summary>
        /// Relays a historical sequence of events.
        /// </summary>
        /// <param name="history">The <see cref="IEnumerable{T}">sequence</see> of historical <see cref="IEvent">events</see> to replay.</param>
        public virtual void ReplayAll( IEnumerable<IEvent> history )
        {
            Arg.NotNull( history, nameof( history ) );

            foreach ( var @event in history )
            {
                Replay( @event );
                Version = @event.Version;
            }

            foreach ( var @event in UncommittedEvents )
            {
                Replay( @event );
            }
        }

        /// <summary>
        /// Creates and returns a new snapshot of the aggregate.
        /// </summary>
        /// <returns>An opaque <see cref="ISnapshot{TKey}">snapshot</see> object for the aggregate based on its current state.</returns>
        /// <remarks>The base implementation always throws <see cref="NotSupportedException"/>.</remarks>
        public virtual ISnapshot<TKey> CreateSnapshot() => throw new NotSupportedException( SR.CreatingSnapshotsUnsupported );

        /// <summary>
        /// Replays the specified event.
        /// </summary>
        /// <param name="event">The event to replay.</param>
        protected virtual void Replay( IEvent @event ) => Dispatcher.Invoke( @event );

        /// <summary>
        /// Records the specified event.
        /// </summary>
        /// <param name="event">The event to record.</param>
        protected virtual void Record( IEvent @event )
        {
            Replay( @event );
            Events.Add( @event );
        }

        /// <summary>
        /// Gets a value indicating whether the aggregate has changed.
        /// </summary>
        /// <value>True if the aggregate has changed; otherwise, false.</value>
        public virtual bool IsChanged => Events.Count > 0;

        /// <summary>
        /// Accepts the changes made to the aggregate.
        /// </summary>
        /// <remarks>This method will update the aggregate <see cref="Version">version</see> as necessary and clear all
        /// <see cref="UncommittedEvents">uncommitted events</see>. This method is typically only called by event stores.</remarks>
        public virtual void AcceptChanges()
        {
            if ( Events.Count == 0 )
            {
                return;
            }

            Version = Events.Last().Version;
            Events.Clear();
        }
    }
}