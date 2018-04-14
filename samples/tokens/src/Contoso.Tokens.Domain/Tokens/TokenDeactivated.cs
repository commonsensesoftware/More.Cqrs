namespace Contoso.Domain.Tokens
{
    using More.Domain.Events;
    using System;

    public class TokenDeactivated : Event<string>
    {
        public TokenDeactivated( string aggregateId, Guid? orderId = null )
        {
            AggregateId = aggregateId;
            OrderId = orderId;
        }

        public Guid? OrderId { get; }
    }
}