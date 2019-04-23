// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using Events;
    using Reflection;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents the base implementation for a repository of aggregates.
    /// </summary>
    /// <typeparam name="TKey">The type of key defined by the aggregate.</typeparam>
    /// <typeparam name="TAggregate">The type of aggregates in the repository.</typeparam>
    public class Repository<TKey, TAggregate> : IRepository<TKey, TAggregate> where TAggregate : class, IAggregate<TKey>
    {
        readonly IEventStore<TKey> eventStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{TKey,TAggregate}"/> class.
        /// </summary>
        /// <param name="eventStore">The underlying <see cref="IEventStore{TKey}">event store</see> used by the repository.</param>
        public Repository( IEventStore<TKey> eventStore )
        {
            Arg.NotNull( eventStore, nameof( eventStore ) );
            this.eventStore = eventStore;
        }

        /// <summary>
        /// Returns a single aggregate from the repository.
        /// </summary>
        /// <param name="id">The unique aggregate identifier.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">cancellation token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <typeparamref name="TAggregate">aggregate</typeparamref> retrieved asynchronously.</returns>
        public virtual async Task<TAggregate> Single( TKey id, CancellationToken cancellationToken )
        {
            var events = await eventStore.Load( id, cancellationToken ).ConfigureAwait( false );
            var aggregate = (TAggregate) FormatterServices.GetUninitializedObject( typeof( TAggregate ) );

            aggregate.ReplayAll( events );
            return aggregate;
        }

        /// <summary>
        /// Saves the specified aggregate.
        /// </summary>
        /// <param name="aggregate">The <typeparamref name="TAggregate">aggregate</typeparamref> to save.</param>
        /// <param name="expectedVersion">The expected version of the saved aggregate.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">cancellation token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual async Task Save( TAggregate aggregate, int expectedVersion, CancellationToken cancellationToken )
        {
            Arg.NotNull( aggregate, nameof( aggregate ) );

            await eventStore.Save( aggregate.Id, aggregate.UncommittedEvents, expectedVersion, cancellationToken ).ConfigureAwait( false );
            aggregate.AcceptChanges();
        }
    }
}