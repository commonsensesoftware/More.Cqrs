namespace The.Simplest.Possible.Thing
{
    using More.Domain;
    using More.Domain.Events;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    class Projector :
        IReceiveEvent<InventoryItemCreated>,
        IReceiveEvent<InventoryItemRenamed>,
        IReceiveEvent<InventoryItemDeactivated>
    {
        private readonly List<Product> products = new List<Product>();

        public IReadOnlyList<Product> Products => products;

        public ValueTask Receive( InventoryItemDeactivated @event, IMessageContext context, CancellationToken cancellationToken )
        {
            var product = products.SingleOrDefault( p => p.Id == @event.AggregateId );

            if ( product != null )
            {
                product.IsActive = false;
            }

            return default;
        }

        public ValueTask Receive( InventoryItemRenamed @event, IMessageContext context, CancellationToken cancellationToken )
        {
            var product = products.SingleOrDefault( p => p.Id == @event.AggregateId );

            if ( product != null )
            {
                product.Name = @event.NewName;
            }

            return default;
        }

        public ValueTask Receive( InventoryItemCreated @event, IMessageContext context, CancellationToken cancellationToken )
        {
            var product = products.SingleOrDefault( p => p.Id == @event.AggregateId );

            if ( product == null )
            {
                products.Add( new Product( @event.AggregateId ) { Name = @event.Name, IsActive = true } );
            }

            return default;
        }
    }
}