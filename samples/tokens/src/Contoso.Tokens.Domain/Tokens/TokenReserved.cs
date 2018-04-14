namespace Contoso.Domain.Tokens
{
    using More.Domain.Events;
    using System;

    public class TokenReserved : Event<string>
    {
        public TokenReserved( string aggregateId, Guid orderId, string billingAccountId, string code, string hash )
        {
            AggregateId = aggregateId;
            OrderId = orderId;
            BillingAccountId = billingAccountId;
            Code = code;
            Hash = hash;
        }

        public Guid OrderId { get; }

        public string BillingAccountId { get; }

        public string Code { get; }

        public string Hash { get; }
    }
}