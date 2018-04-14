namespace Contoso.Domain.Tokens.Ordering
{
    using More.Domain.Events;
    using System;

    public class OrderCanceled : Event
    {
        public OrderCanceled( Guid aggregateId ) => AggregateId = aggregateId;
    }
}