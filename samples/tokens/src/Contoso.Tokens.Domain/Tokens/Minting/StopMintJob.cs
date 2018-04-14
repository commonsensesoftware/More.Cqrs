namespace Contoso.Domain.Tokens.Minting
{
    using More.Domain.Commands;
    using System;

    public class StopMintJob : Command
    {
        public StopMintJob( Guid aggregateId, int jobId )
        {
            AggregateId = aggregateId;
            JobId = jobId;
        }

        public int JobId { get; }
    }
}