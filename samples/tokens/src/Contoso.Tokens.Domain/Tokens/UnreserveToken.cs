namespace Contoso.Domain.Tokens
{
    using More.Domain.Commands;
    using System;

    public class UnreserveToken : Command<string>
    {
        public UnreserveToken( string aggregateId, int expectedVersion, Guid orderId )
        {
            AggregateId = aggregateId;
            ExpectedVersion = expectedVersion;
            OrderId = orderId;
        }

        public Guid OrderId { get; }
    }
}