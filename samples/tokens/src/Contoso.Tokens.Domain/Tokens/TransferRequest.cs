namespace Contoso.Domain.Tokens
{
    using System;

    public class TransferRequest
    {
        public TransferRequest( Guid orderId, string billingAccountId, string catalogId, int quantity, bool activateImmediately, string correlationId )
        {
            OrderId = orderId;
            BillingAccountId = billingAccountId;
            CatalogId = catalogId;
            Quantity = quantity;
            ActivateImmediately = activateImmediately;
            CorrelationId = correlationId;
        }

        public Guid OrderId { get; }

        public string BillingAccountId { get; }

        public string CatalogId { get; }

        public int Quantity { get; }

        public bool ActivateImmediately { get; }

        public string CorrelationId { get; }
    }
}