namespace More.Domain.Sagas
{
    using More.Domain.Events;
    using System;

    public class SalesOrderReceived : Event
    {
        public SalesOrderReceived( Guid aggregateId ) => AggregateId  = aggregateId;
    }
}