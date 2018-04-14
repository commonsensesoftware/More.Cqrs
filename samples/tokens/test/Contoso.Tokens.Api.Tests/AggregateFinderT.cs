namespace Contoso.Services
{
    using More.Domain.Events;

    public class AggregateFinder<TKey>
    {
        readonly IEventStore<TKey> eventStore;

        public AggregateFinder( IEventStore<TKey> eventStore ) => this.eventStore = eventStore;

        public AggregatePredicate<TKey> WithId( TKey id ) => new AggregatePredicate<TKey>( eventStore, id );
    }
}