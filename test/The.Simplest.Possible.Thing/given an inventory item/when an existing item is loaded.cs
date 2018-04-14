namespace given_an_inventory_item
{
    using FluentAssertions;
    using More.Domain.Events;
    using System;
    using The.Simplest.Possible.Thing;
    using Xunit;

    public class when_an_existing_item_is_loaded
    {
        [Fact]
        public void then_it_should_reify_from_historical_events()
        {
            // arrange
            var id = Guid.NewGuid();
            var history = new IEvent[]
            {
                new InventoryItemCreated( id, "Foo" ) { Version = 0 },
                new InventoryItemRenamed( id, "Foo Bar" ) { Version = 1 },
                new InventoryItemDeactivated( id ) { Version = 2 }
            };
            var item = new InventoryItem();

            // act
            item.ReplayAll( history );

            // assert
            item.UncommittedEvents.Should().BeEmpty();
            item.Version.Should().Be( 2 );
        }
    }
}