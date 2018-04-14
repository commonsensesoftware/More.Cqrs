namespace Contoso.Domain.Tokens.Printing
{
    using More.Domain.Events;
    using System;

    public class PrintJobQueued : Event
    {
        public PrintJobQueued(
            Guid aggregateId,
            string billingAccountId,
            string catalogId,
            long quantity,
            string certificateThumbprint,
            string idempotencyToken )
        {
            AggregateId = aggregateId;
            BillingAccountId = billingAccountId;
            CatalogId = catalogId;
            Quantity = quantity;
            CertificateThumbprint = certificateThumbprint;
            IdempotencyToken = idempotencyToken;
        }

        public string BillingAccountId { get; }

        public string CatalogId { get; }

        public long Quantity { get; }

        public string CertificateThumbprint { get; }

        public string IdempotencyToken { get; }
    }
}