namespace given_a_product_projector
{
    using FluentAssertions;
    using More.Domain;
    using More.Domain.Commands;
    using More.Domain.Events;
    using More.Domain.Messaging;
    using More.Domain.Persistence;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using The.Simplest.Possible.Thing;
    using Xunit;

    public class when_an_event_is_received : IDisposable
    {
        public when_an_event_is_received()
        {
            Bus = new InMemoryMessageBus();

            var config = Bus.Configuration;
            var persistence = new InMemoryPersistence( config.MessageSender, config.Sagas.Storage );
            var repository = new Repository<InventoryItem>( new InMemoryEventStore<Guid>( persistence ) );
            var commandHandlers = new InventoryCommandHandlers( repository );
            var projector = new Projector();

            config.CommandHandlers.Register<CreateInventoryItem>( () => commandHandlers );
            config.CommandHandlers.Register<RenameInventoryItem>( () => commandHandlers );
            config.CommandHandlers.Register<DeactivateInventoryItem>( () => commandHandlers );
            config.EventReceivers.Register<InventoryItemCreated>( () => projector );
            config.EventReceivers.Register<InventoryItemRenamed>( () => projector );
            config.EventReceivers.Register<InventoryItemDeactivated>( () => projector );
            Products = projector.Products;
            Bus.Start();
        }

        MessageBus Bus { get; }

        IReadOnlyList<Product> Products { get; }

        public void Dispose() => Bus.Dispose();

        [Fact]
        public async Task then_a_product_is_created()
        {
            // arrange
            var itemId = Guid.NewGuid();

            // act
            await Bus.Send( new CreateInventoryItem( itemId, "Foo" ) );
            await Bus.Flush();

            // assert
            Products.Single().Should().BeEquivalentTo(
                new
                {
                    Id = itemId,
                    Name = "Foo",
                    IsActive = true
                } );
        }

        [Fact]
        public async Task then_a_product_is_renamed()
        {
            // arrange
            var itemId = Guid.NewGuid();

            // act
            await Bus.Send( new CreateInventoryItem( itemId, "Foo" ) );
            await Bus.Send( new RenameInventoryItem( itemId, "Foo Bar", 0 ) );
            await Bus.Flush();

            // assert
            Products.Single().Should().BeEquivalentTo(
                new
                {
                    Id = itemId,
                    Name = "Foo Bar",
                    IsActive = true
                } );
        }

        [Fact]
        public async Task then_a_product_should_be_deactivated()
        {
            // arrange
            var itemId = Guid.NewGuid();

            // act
            await Bus.Send( new CreateInventoryItem( itemId, "Foo" ) );
            await Bus.Send( new RenameInventoryItem( itemId, "Foo Bar", 0 ) );
            await Bus.Send( new DeactivateInventoryItem( itemId, 1 ) );
            await Bus.Flush();

            // assert
            Products.Single().Should().BeEquivalentTo(
                new
                {
                    Id = itemId,
                    Name = "Foo Bar",
                    IsActive = false
                } );
        }
    }
}