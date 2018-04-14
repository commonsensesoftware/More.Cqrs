namespace Contoso.Domain.Tokens.Minting
{
    using More.Domain.Events;
    using System;
    using System.Collections.Generic;

    public class MintRequested : Event
    {
        public MintRequested( Guid aggregateId, string catalogId, string idempotencyToken )
        {
            AggregateId = aggregateId;
            CatalogId = catalogId;
            IdempotencyToken = idempotencyToken;
        }

        public string CatalogId { get; }

        public string IdempotencyToken { get; }

        public IList<MintJob> MintJobs { get; } = new List<MintJob>();
    }
}