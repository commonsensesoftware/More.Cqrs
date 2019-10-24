namespace More.Domain.Sagas
{
    using More.Domain.Events;
    using System.Threading;
    using System.Threading.Tasks;

    public class Procurement : Saga<ProcurementData>,
        IStartWith<SubmitPurchaseOrder>,
        IReceiveEvent<SalesOrderReceived>,
        IReceiveEvent<ShipmentReceived>
    {
        protected override void CorrelateUsing( SagaCorrelator<ProcurementData> correlator )
        {
            correlator.Correlate<SubmitPurchaseOrder>( command => command.AggregateId ).To( saga => saga.OrderId );
            correlator.Correlate<SalesOrderReceived>( @event => @event.AggregateId ).To( saga => saga.OrderId );
            correlator.Correlate<ShipmentReceived>( @event => @event.AggregateId ).To( saga => saga.OrderId );
        }

        public ValueTask Handle( SubmitPurchaseOrder command, IMessageContext context, CancellationToken cancellation )
        {
            Data.OrderId = command.AggregateId;
            return default;
        }

        public ValueTask Receive( ShipmentReceived @event, IMessageContext context, CancellationToken cancellation )
        {
            MarkAsComplete();
            return default;
        }

        public ValueTask Receive( SalesOrderReceived @event, IMessageContext context, CancellationToken cancellation ) => default;
    }
}