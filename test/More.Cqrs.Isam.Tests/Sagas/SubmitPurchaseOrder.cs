namespace More.Domain.Sagas
{
    using More.Domain.Commands;
    using System;

    public class SubmitPurchaseOrder : Command
    {
        public SubmitPurchaseOrder( Guid aggregateId ) => AggregateId  = aggregateId;
    }
}