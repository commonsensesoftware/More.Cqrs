namespace More.Domain.Sagas
{
    using More.Domain.Events;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

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

        public Task Handle( SubmitPurchaseOrder command, IMessageContext context )
        {
            Data.OrderId = command.AggregateId;
            return CompletedTask;
        }

        public Task Receive( ShipmentReceived @event, IMessageContext context )
        {
            MarkAsComplete();
            return CompletedTask;
        }

        public Task Receive( SalesOrderReceived @event, IMessageContext context ) => CompletedTask;
    }
}