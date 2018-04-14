namespace More.Domain.Sagas
{
    using System;

    public class ProcurementData : SagaData
    {
        public Guid OrderId { get; set; }
    }
}