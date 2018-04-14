namespace given_an_inventory_item
{
    using FluentAssertions;
    using System;
    using System.Linq;
    using The.Simplest.Possible.Thing;
    using Xunit;

    public class when_a_new_item_is_created
    {
        [Fact]
        public void then_a_single_event_should_be_recorded()
        {
            // arrange
            var id = Guid.NewGuid();
            var item = default( InventoryItem );

            // act
            item = new InventoryItem( id, "Test" );

            // assert
            item.UncommittedEvents.Single().Should().BeEquivalentTo( new InventoryItemCreated( id, "Test" ) );
        }
    }
}