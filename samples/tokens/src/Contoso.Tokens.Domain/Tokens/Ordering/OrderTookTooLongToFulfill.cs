namespace Contoso.Domain.Tokens.Ordering
{
    using More.Domain.Events;
    using System;

    public class OrderTookTooLongToFulfill : Event
    {
        public OrderTookTooLongToFulfill( Guid aggregateId ) => AggregateId = aggregateId;
    }
}