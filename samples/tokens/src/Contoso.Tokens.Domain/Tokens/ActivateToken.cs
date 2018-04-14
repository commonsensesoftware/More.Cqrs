namespace Contoso.Domain.Tokens
{
    using More.Domain.Commands;
    using System;

    public class ActivateToken : Command<string>
    {
        public ActivateToken( string aggregateId, int expectedVersion, string billingAccountId, Guid? orderId = null )
        {
            AggregateId = aggregateId;
            ExpectedVersion = expectedVersion;
            BillingAccountId = billingAccountId;
            OrderId = orderId;
        }

        public string BillingAccountId { get; }

        public Guid? OrderId { get; }
    }
}