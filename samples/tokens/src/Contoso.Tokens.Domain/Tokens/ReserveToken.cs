namespace Contoso.Domain.Tokens
{
    using More.Domain.Commands;
    using System;

    public class ReserveToken : Command
    {
        public ReserveToken( Guid aggregateId, string billingAccountId, string catalogId )
        {
            AggregateId = aggregateId;
            BillingAccountId = billingAccountId;
            CatalogId = catalogId;
        }

        public string BillingAccountId { get; }

        public string CatalogId { get; }
    }
}