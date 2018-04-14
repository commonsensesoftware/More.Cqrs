namespace Contoso.Domain.Tokens.Printing
{
    using More.Domain.Commands;
    using System;

    public class Print : Command
    {
        public Print(
            Guid aggregateId,
            string billingAccountId,
            string catalogId,
            int quantity,
            string certificateThumbprint,
            string idempotencyToken,
            string correlationId )
        {
            AggregateId = aggregateId;
            BillingAccountId = billingAccountId;
            CatalogId = catalogId;
            Quantity = quantity;
            CertificateThumbprint = certificateThumbprint;
            IdempotencyToken = idempotencyToken;
            CorrelationId = correlationId;
        }

        public string IdempotencyToken { get; }

        public string BillingAccountId { get; }

        public string CatalogId { get; }

        public int Quantity { get; }

        public string CertificateThumbprint { get; }
    }
}