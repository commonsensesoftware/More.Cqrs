namespace Contoso.Domain.Tokens.Printing
{
    using More.Domain.Commands;
    using System;

    public class CancelPrintJob : Command
    {
        public CancelPrintJob( Guid aggregateId, int expectedVersion )
        {
            AggregateId = aggregateId;
            ExpectedVersion = expectedVersion;
        }
    }
}