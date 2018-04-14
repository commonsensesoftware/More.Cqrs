namespace Contoso.Domain.Tokens.Minting
{
    using More.Domain.Commands;
    using System;

    public class CancelMint : Command
    {
        public CancelMint( Guid aggregateId, int expectedVersion )
        {
            AggregateId = aggregateId;
            ExpectedVersion = expectedVersion;
        }
    }
}