namespace The.Simplest.Possible.Thing
{
    using More.Domain;
    using More.Domain.Events;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    class Projector : IReceiveEvent<InventoryItemCreated>, IReceiveEvent<InventoryItemRenamed>, IReceiveEvent<InventoryItemDeactivated>
    {
        private readonly List<Product> products = new List<Product>();

        public IReadOnlyList<Product> Products => products;

        public Task Receive( InventoryItemDeactivated @event, IMessageContext context )
        {
            var product = products.SingleOrDefault( p => p.Id == @event.AggregateId );

            if ( product != null )
            {
                product.IsActive = false;
            }

            return CompletedTask;
        }

        public Task Receive( InventoryItemRenamed @event, IMessageContext context )
        {
            var product = products.SingleOrDefault( p => p.Id == @event.AggregateId );

            if ( product != null )
            {
                product.Name = @event.NewName;
            }

            return CompletedTask;
        }

        public Task Receive( InventoryItemCreated @event, IMessageContext context )
        {
            var product = products.SingleOrDefault( p => p.Id == @event.AggregateId );

            if ( product == null )
            {
                products.Add( new Product( @event.AggregateId ) { Name = @event.Name, IsActive = true } );
            }

            return CompletedTask;
        }
    }
}