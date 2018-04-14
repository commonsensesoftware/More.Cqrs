namespace Contoso.Domain.Tokens.Ordering
{
    using More.Domain.Commands;
    using System;

    public class PlaceOrder : Command
    {
        public PlaceOrder(
            Guid aggregateId,
            string billingAccountId,
            string catalogId,
            int quantity,
            bool activateImmediately,
            string idempotencyToken,
            string correlationId )
        {
            AggregateId = aggregateId;
            BillingAccountId = billingAccountId;
            CatalogId = catalogId;
            Quantity = quantity;
            ActivateImmediately = activateImmediately;
            IdempotencyToken = idempotencyToken;
            CorrelationId = correlationId;
        }

        public string IdempotencyToken { get; }

        public string BillingAccountId { get; }

        public string CatalogId { get; }

        public int Quantity { get; }

        public bool ActivateImmediately { get; }
    }
}