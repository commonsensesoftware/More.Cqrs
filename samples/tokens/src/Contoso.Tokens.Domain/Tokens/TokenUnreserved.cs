namespace Contoso.Domain.Tokens
{
    using More.Domain.Events;
    using System;

    public class TokenUnreserved : Event<string>
    {
        public TokenUnreserved( string aggregateId, Guid orderId )
        {
            AggregateId = aggregateId;
            OrderId = orderId;
        }

        public Guid OrderId { get; }
    }
}