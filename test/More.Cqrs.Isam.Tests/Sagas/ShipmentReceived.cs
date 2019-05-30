namespace More.Domain.Sagas
{
    using More.Domain.Events;
    using System;

    public class ShipmentReceived : Event
    {
        public ShipmentReceived( Guid aggregateId ) => AggregateId = aggregateId;
    }
}