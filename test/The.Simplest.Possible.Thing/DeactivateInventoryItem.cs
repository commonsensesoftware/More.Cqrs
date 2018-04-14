namespace The.Simplest.Possible.Thing
{
    using More.Domain.Commands;
    using System;

    class DeactivateInventoryItem : Command
    {
        public DeactivateInventoryItem( Guid itemId, int originalVersion )
        {
            AggregateId = itemId;
            ExpectedVersion = originalVersion;
        }
    }
}