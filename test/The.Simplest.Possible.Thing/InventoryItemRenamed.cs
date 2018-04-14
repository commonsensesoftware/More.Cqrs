namespace The.Simplest.Possible.Thing
{
    using More.Domain.Events;
    using System;

    class InventoryItemRenamed : Event
    {
        public InventoryItemRenamed( Guid id, string newName )
        {
            AggregateId = id;
            NewName = newName;
        }

        public string NewName { get; }
    }
}