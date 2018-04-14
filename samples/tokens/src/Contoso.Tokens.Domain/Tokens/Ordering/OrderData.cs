namespace Contoso.Domain.Tokens.Ordering
{
    using More.Domain.Sagas;
    using System;

    public class OrderData : SagaData
    {
        public string IdempotencyToken { get; set; }

        public OrderState State { get; set; }

        public string BillingAccountId { get; set; }

        public string CatalogId { get; set; }

        public int QuantityOrdered { get; set; }

        public bool ActivateImmediately { get; set; }

        public DateTimeOffset StartedOn { get; set; }

        public bool TimedOut { get; set; }
    }
}