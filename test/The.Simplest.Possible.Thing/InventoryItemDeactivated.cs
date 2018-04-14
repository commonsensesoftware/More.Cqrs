namespace The.Simplest.Possible.Thing
{
    using More.Domain.Events;
    using System;

    class InventoryItemDeactivated : Event
    {
        public InventoryItemDeactivated( Guid id ) => AggregateId = id;
    }
}