namespace Contoso.Services
{
    using More.Domain.Events;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.TimeSpan;

    public class AggregatePredicate<TKey>
    {
        readonly IEventStore<TKey> eventStore;
        readonly TKey key;

        public AggregatePredicate( IEventStore<TKey> eventStore, TKey key )
        {
            this.eventStore = eventStore;
            this.key = key;
        }

        public Task ToRecordEvent<TEvent>() where TEvent : class, IEvent => ToRecordEvent<TEvent>( 10000 );

        public async Task ToRecordEvent<TEvent>( int timeOutInMilliseconds ) where TEvent : class, IEvent
        {
#if DEBUG
            if ( Debugger.IsAttached )
            {
                timeOutInMilliseconds += (int) FromMinutes( 5d ).TotalMilliseconds;
            }
#endif

            using ( var cts = new CancellationTokenSource( timeOutInMilliseconds ) )
            {
                await ToRecordEvent<TEvent>( cts.Token );
            }
        }

        public async Task ToRecordEvent<TEvent>( CancellationToken cancellationToken ) where TEvent : class, IEvent
        {
            var completed = false;
            var events = default( IEnumerable<IEvent> );

            do
            {
                try
                {
                    events = await eventStore.Load( key, cancellationToken );
                }
                catch ( OperationCanceledException )
                {
                    throw new TimeoutException( $"The operation timed out while waiting for the {typeof( TEvent ).Name} event on an aggregate with id {key}." );
                }
                catch ( InvalidOperationException )
                {
                    throw new TimeoutException( $"The operation timed out while waiting for the {typeof( TEvent ).Name} event on an aggregate with id {key}." );
                }

                completed = events.Reverse().OfType<TEvent>().Any();
            }
            while ( !completed );
        }
    }
}