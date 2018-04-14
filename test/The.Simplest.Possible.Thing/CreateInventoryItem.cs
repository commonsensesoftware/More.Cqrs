namespace The.Simplest.Possible.Thing
{
    using More.Domain.Commands;
    using System;

    class CreateInventoryItem : Command
    {
        public CreateInventoryItem( Guid itemId, string name )
        {
            AggregateId = itemId;
            Name = name;
        }

        public string Name { get; }
    }
}