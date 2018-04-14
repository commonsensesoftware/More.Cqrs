namespace Contoso.Domain.Tokens.Minting
{
    using More.Domain.Commands;
    using System;

    public class Mint : Command
    {
        public Mint( Guid aggregateId, string billingAccountId, string catalogId, long quantity, string idempotencyToken, string correlationId )
        {
            AggregateId = aggregateId;
            BillingAccountId = billingAccountId;
            CatalogId = catalogId;
            Quantity = quantity;
            IdempotencyToken = idempotencyToken;
            CorrelationId = correlationId;
        }

        public string BillingAccountId { get; }

        public string CatalogId { get; }

        public long Quantity { get; }

        public string IdempotencyToken { get; set; }
    }
}