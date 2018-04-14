namespace Contoso.Domain.Tokens.Minting
{
    using More.Domain.Events;
    using System;

    public class MintJobFinished : Event
    {
        public MintJobFinished( Guid aggregateId, int jobId )
        {
            AggregateId = aggregateId;
            JobId = jobId;
        }

        public int JobId { get; }
    }
}