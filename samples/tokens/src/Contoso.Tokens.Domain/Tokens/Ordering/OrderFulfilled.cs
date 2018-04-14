namespace Contoso.Domain.Tokens.Ordering
{
    using More.Domain.Events;
    using System;

    public class OrderFulfilled : Event
    {
        public OrderFulfilled( Guid aggregateId ) => AggregateId = aggregateId;
    }
}