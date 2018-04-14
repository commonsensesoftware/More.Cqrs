namespace Contoso.Domain.Tokens.Ordering
{
    using More.Domain.Events;
    using System;

    public class OrderPlaced : Event
    {
        public OrderPlaced(
            Guid aggregateId,
            string billingAccountId,
            string catalogId,
            int quantity,
            bool activateImmediately,
            DateTimeOffset receivedDate,
            string idempotencyToken )
        {
            AggregateId = aggregateId;
            BillingAccountId = billingAccountId;
            CatalogId = catalogId;
            Quantity = quantity;
            ActivateImmediately = activateImmediately;
            ReceivedDate = receivedDate;
            IdempotencyToken = idempotencyToken;
        }

        public string BillingAccountId { get; }

        public string CatalogId { get; }

        public int Quantity { get; }

        public bool ActivateImmediately { get; }

        public DateTimeOffset ReceivedDate { get; }

        public string IdempotencyToken { get; }
    }
}