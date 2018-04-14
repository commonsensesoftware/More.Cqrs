namespace The.Simplest.Possible.Thing
{
    using More.Domain.Commands;
    using System;

    class RenameInventoryItem : Command
    {
        public RenameInventoryItem( Guid itemId, string newName, int originalVersion )
        {
            AggregateId = itemId;
            NewName = newName;
            ExpectedVersion = originalVersion;
        }

        public string NewName { get; }
    }
}