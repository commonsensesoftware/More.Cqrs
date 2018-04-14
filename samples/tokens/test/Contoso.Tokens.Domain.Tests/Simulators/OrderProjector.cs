namespace Contoso.Domain.Simulators
{
    using Contoso.Domain.Tokens.Ordering;
    using More.Domain;
    using More.Domain.Events;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using static Contoso.Tokens.OrderState;
    using static System.Threading.Tasks.Task;
    using Order = Contoso.Tokens.Order;
    using Token = Contoso.Tokens.Token;
    using OrderState = Contoso.Tokens.OrderState;
    using TokenReserved = Contoso.Domain.Tokens.TokenReserved;
    using TokenUnreserved = Contoso.Domain.Tokens.TokenUnreserved;

    class OrderProjector :
        IReceiveEvent<OrderPlaced>,
        IReceiveEvent<OrderFulfilled>,
        IReceiveEvent<OrderCanceled>,
        IReceiveEvent<TokenReserved>,
        IReceiveEvent<TokenUnreserved>
    {
        readonly List<Order> orders = new List<Order>();

        public IReadOnlyList<Order> Orders => orders;

        public Task Receive( OrderCanceled @event, IMessageContext context )
        {
            TransitionState( @event, Canceled );
            return CompletedTask;
        }

        public Task Receive( OrderFulfilled @event, IMessageContext context )
        {
            TransitionState( @event, Fulfilled );
            return CompletedTask;
        }

        public Task Receive( OrderPlaced @event, IMessageContext context )
        {
            var order = orders.FirstOrDefault( o => o.Id == @event.AggregateId );

            if ( order == null )
            {
                orders.Add(
                    new Order()
                    {
                        Id = @event.AggregateId,
                        BillingAccountId = @event.BillingAccountId,
                        CatalogId = @event.CatalogId,
                        Quantity = @event.Quantity,
                        State = Placed,
                        Version = @event.Version
                    } );
            }
            else
            {
                order.BillingAccountId = @event.BillingAccountId;
                order.CatalogId = @event.CatalogId;
                order.Quantity = @event.Quantity;
            }

            return CompletedTask;
        }

        public Task Receive( TokenReserved @event, IMessageContext context )
        {
            var order = orders.FirstOrDefault( o => o.Id == @event.OrderId );

            if ( order != null )
            {
                if ( order.State != Canceled )
                {
                    order.State = Processing;
                }

                order.Tokens.Add( new Token() { Id = @event.AggregateId } );
            }

            return CompletedTask;
        }

        public Task Receive( TokenUnreserved @event, IMessageContext context )
        {
            var order = orders.FirstOrDefault( o => o.Id == @event.OrderId );

            if ( order != null )
            {
                for ( var i = order.Tokens.Count - 1; i >= 0; i-- )
                {
                    var token = order.Tokens[i];

                    if ( token.Id == @event.AggregateId )
                    {
                        order.Tokens.RemoveAt( i );
                        break;
                    }
                }
            }

            return CompletedTask;
        }

        void TransitionState( Event<Guid> @event, OrderState state )
        {
            var order = orders.FirstOrDefault( o => o.Id == @event.AggregateId );

            if ( order == null )
            {
                orders.Add( new Order() { Id = @event.AggregateId, State = state, Version = @event.Version } );
            }
            else
            {
                order.State = state;
            }
        }
    }
}