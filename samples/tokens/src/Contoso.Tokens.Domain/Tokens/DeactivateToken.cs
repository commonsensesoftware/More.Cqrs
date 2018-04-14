namespace Contoso.Domain.Tokens
{
    using More.Domain.Commands;
    using System;

    public class DeactivateToken : Command<string>
    {
        public DeactivateToken( string aggregateId, int expectedVersion, Guid? orderId = null )
        {
            AggregateId = aggregateId;
            ExpectedVersion = expectedVersion;
        }

        public Guid? OrderId { get; }
    }
}