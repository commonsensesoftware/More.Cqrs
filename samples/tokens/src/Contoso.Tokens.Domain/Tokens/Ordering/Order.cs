namespace Contoso.Domain.Tokens.Ordering
{
    using More.Domain;
    using More.Domain.Commands;
    using More.Domain.Events;
    using More.Domain.Sagas;
    using System;
    using System.Threading.Tasks;
    using static OrderState;
    using static System.Threading.Tasks.Task;
    using static System.DateTimeOffset;

    public class Order : Saga<OrderData>,
        IStartWith<PlaceOrder>,
        IReceiveEvent<OrderPlaced>,
        IHandleCommand<CancelOrder>,
        ITimeoutWhen<OrderTookTooLongToFulfill>
    {
        protected override void CorrelateUsing( SagaCorrelator<OrderData> correlator )
        {
            correlator.Correlate<PlaceOrder>( command => command.AggregateId ).To( saga => saga.Id );
            correlator.Correlate<OrderPlaced>( @event => @event.AggregateId ).To( saga => saga.Id );
            correlator.Correlate<CancelOrder>( command => command.AggregateId ).To( saga => saga.Id );
            correlator.Correlate<OrderTookTooLongToFulfill>( @event => @event.AggregateId ).To( saga => saga.Id );
        }

        public async Task Handle( PlaceOrder command, IMessageContext context )
        {
            if ( Data.IdempotencyToken == command.IdempotencyToken )
            {
                return;
            }

            var receivedDate = Now;
            var afterOneDay = receivedDate.AddDays( 1d );
            var orderId = command.AggregateId;
            var billingAccountId = command.BillingAccountId;
            var catalogId = command.CatalogId;
            var orderPlaced = new OrderPlaced( orderId,
                                               billingAccountId,
                                               catalogId,
                                               command.Quantity,
                                               command.ActivateImmediately,
                                               receivedDate,
                                               command.IdempotencyToken );

            Record( orderPlaced );
            await RequestTimeout( afterOneDay, new OrderTookTooLongToFulfill( orderId ), context );
        }

        public async Task Receive( OrderPlaced @event, IMessageContext context )
        {
            if ( Data.State == Canceled )
            {
                return;
            }

            var tokenVault = (ITokenVault) context.GetService( typeof( ITokenVault ) );
            var request = new TransferRequest( @event.AggregateId, @event.BillingAccountId, @event.CatalogId, @event.Quantity, @event.ActivateImmediately, @event.CorrelationId );
            var success = await tokenVault.Transfer( request, context.CancellationToken );

            if ( success )
            {
                Record( new OrderFulfilled( Id ) );
                MarkAsComplete();
            }
            else
            {
                // TODO: handle and reply with event when reservation cannot be satisfied; retry should be possible
                await Handle( new CancelOrder( Id, Version ), context );
            }
        }

        public async Task Handle( CancelOrder command, IMessageContext context )
        {
            if ( Completed )
            {
                return;
            }

            var tokenVault = (ITokenVault) context.GetService( typeof( ITokenVault ) );

            await tokenVault.ReverseTransfer( command.AggregateId, command.CorrelationId, context.CancellationToken );
            Record( new OrderCanceled( Id ) );
            MarkAsComplete();
        }

        public Task Receive( OrderTookTooLongToFulfill @event, IMessageContext context )
        {
            if ( Completed )
            {
                return CompletedTask;
            }

            Data.TimedOut = true;
            return Handle( new CancelOrder( Id, Version ), context );
        }

        void Apply( OrderPlaced @event )
        {
            Data.Id = @event.AggregateId;
            Data.State = Placed;
            Data.BillingAccountId = @event.BillingAccountId;
            Data.CatalogId = @event.CatalogId;
            Data.QuantityOrdered = @event.Quantity;
            Data.ActivateImmediately = @event.ActivateImmediately;
            Data.StartedOn = @event.ReceivedDate;
            Data.IdempotencyToken = @event.IdempotencyToken;
        }

        void Apply( OrderFulfilled @event ) => Data.State = Fulfilled;

        void Apply( OrderCanceled @event ) => Data.State = Canceled;
    }
}