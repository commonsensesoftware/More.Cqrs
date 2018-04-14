namespace The.Simplest.Possible.Thing
{
    using More.Domain;
    using System;

    class InventoryItem : Aggregate
    {
        bool active;
        string name;

        public InventoryItem() { }

        public InventoryItem( Guid id, string name ) => Record( new InventoryItemCreated( id, name ) );

        public void Deactivate()
        {
            if ( !active )
            {
                Record( new InventoryItemDeactivated( Id ) );
            }
        }

        public void Rename( string newName )
        {
            if ( name != newName )
            {
                Record( new InventoryItemRenamed( Id, newName ) );
            }
        }

        private void Apply( InventoryItemCreated @event )
        {
            Id = @event.AggregateId;
            name = @event.Name;
        }

        private void Apply( InventoryItemDeactivated @event ) => active = false;

        private void Apply( InventoryItemRenamed @event ) => name = @event.NewName;
    }
}