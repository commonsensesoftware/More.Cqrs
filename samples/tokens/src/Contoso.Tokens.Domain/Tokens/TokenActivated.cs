namespace Contoso.Domain.Tokens
{
    using More.Domain.Events;
    using System;

    public class TokenActivated : Event<string>
    {
        public TokenActivated( string aggregateId, string billingAccountId, Guid? orderId = null )
        {
            AggregateId = aggregateId;
            BillingAccountId = billingAccountId;
            OrderId = orderId;
        }

        public Guid? OrderId { get; }

        public string BillingAccountId { get; }
    }
}