// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Persistence;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Globalization.CultureInfo;
    using static System.Linq.Enumerable;

    /// <summary>
    /// Represents an in-memory <see cref="IEventStore{TKey}">event store</see>.
    /// </summary>
    /// <typeparam name="TKey">The type of event key.</typeparam>
    public class InMemoryEventStore<TKey> : EventStore<TKey> where TKey : notnull
    {
        readonly ConcurrentDictionary<TKey, List<IEvent>> table = new ConcurrentDictionary<TKey, List<IEvent>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventStore{TKey}"/> class.
        /// </summary>
        /// <param name="persistence">The <see cref="IPersistence">persistence</see> for the event store.</param>
        public InMemoryEventStore( IPersistence persistence ) : base( persistence ) { }

        /// <summary>
        /// Occurs when a sequence of events for an aggregate are loaded.
        /// </summary>
        /// <param name="aggregateId">The identifier of the aggregate to load the events for.</param>
        /// <param name="predicate">The optional <see cref="IEventPredicate{TKey}">predicate</see> used to filter events.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <see cref="IEnumerable{T}">sequence</see> of
        /// <see cref="IEvent">events</see> loaded for the aggregate.</returns>
        protected override async IAsyncEnumerable<IEvent> OnLoad( TKey aggregateId, IEventPredicate<TKey>? predicate )
        {
            IQueryable<IEvent> query;

            if ( table.TryGetValue( aggregateId, out var events ) )
            {
                query = events.AsQueryable();

                if ( predicate != null )
                {
                    query = ApplyPredicate( query, predicate );
                }
            }
            else
            {
                query = Empty<IEvent>().AsQueryable();
            }

            await Task.Yield();

            foreach ( var @event in query )
            {
                yield return @event;
            }
        }

        /// <summary>
        /// Saves a sequence of events for an aggregate.
        /// </summary>
        /// <param name="aggregateId">The identifier of the aggregate to save the events for.</param>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> to save.</param>
        /// <param name="expectedVersion">The expected version of the aggregate being saved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public override async Task Save( TKey aggregateId, IEnumerable<IEvent> events, int expectedVersion, CancellationToken cancellationToken )
        {
            await base.Save( aggregateId, events, expectedVersion, cancellationToken ).ConfigureAwait( false );
            table.GetOrAdd( aggregateId, key => new List<IEvent>() ).AddRange( events );
        }

        private static IQueryable<IEvent> ApplyPredicate( IQueryable<IEvent> query, IEventPredicate<TKey> predicate )
        {
            if ( predicate is EventVersionPredicate<TKey> versionPredicate )
            {
                var version = versionPredicate.Version;
                return query.Where( e => e.Version >= version );
            }
            else if ( predicate is EventDateRangePredicate<TKey> datePredicate )
            {
                if ( datePredicate.To == null )
                {
                    var from = datePredicate.From!.Value;
                    return query.Where( e => e.RecordedOn >= from );
                }
                else if ( datePredicate.From == null )
                {
                    var to = datePredicate.To.Value;
                    return query.Where( e => e.RecordedOn <= to );
                }
                else
                {
                    var from = datePredicate.From.Value;
                    var to = datePredicate.To.Value;

                    return query.Where( e => e.RecordedOn >= from && e.RecordedOn <= to );
                }
            }

            var message = string.Format( CurrentCulture, SR.PredicateNotSupported, predicate.GetType().Name );
            throw new UnsupportedEventPredicateException( message );
        }
    }
}