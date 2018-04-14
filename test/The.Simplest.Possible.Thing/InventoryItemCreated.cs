namespace The.Simplest.Possible.Thing
{
    using More.Domain.Events;
    using System;

    class InventoryItemCreated : Event
    {
        public InventoryItemCreated( Guid id, string name )
        {
            AggregateId = id;
            Name = name;
        }

        public string Name { get; }
    }
}