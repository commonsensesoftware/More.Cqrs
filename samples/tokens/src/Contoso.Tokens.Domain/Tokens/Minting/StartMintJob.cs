namespace Contoso.Domain.Tokens.Minting
{
    using More.Domain.Commands;
    using System;

    public class StartMintJob : Command
    {
        public StartMintJob(
            Guid aggregateId,
            int expectedVersion,
            int jobId,
            string catalogId,
            long startOfSequence,
            long count )
        {
            AggregateId = aggregateId;
            ExpectedVersion = expectedVersion;
            JobId = jobId;
            CatalogId = catalogId;
            StartOfSequence = startOfSequence;
            Count = count;
        }

        public int JobId { get; }

        public string CatalogId { get; }

        public long StartOfSequence { get; }

        public long Count { get; }
    }
}