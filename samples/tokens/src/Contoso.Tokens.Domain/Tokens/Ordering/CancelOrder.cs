namespace Contoso.Domain.Tokens.Ordering
{
    using More.Domain.Commands;
    using System;

    public class CancelOrder : Command
    {
        public CancelOrder( Guid aggregateId, int expectedVersion )
        {
            AggregateId = aggregateId;
            ExpectedVersion = expectedVersion;
        }
    }
}