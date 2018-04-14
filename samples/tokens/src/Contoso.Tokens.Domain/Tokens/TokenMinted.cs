namespace Contoso.Domain.Tokens
{
    using More.Domain.Events;
    using System;

    public class TokenMinted : Event<string>
    {
        public TokenMinted( string aggregateId, Guid mintRequestId, string code, string hash, string catalogId, long sequenceNumber )
        {
            AggregateId = aggregateId;
            MintRequestId = mintRequestId;
            Code = code;
            Hash = hash;
            CatalogId = catalogId;
            SequenceNumber = sequenceNumber;
        }

        public Guid MintRequestId { get; }

        public string Code { get; }

        public string Hash { get; }

        public string CatalogId { get; }

        public long SequenceNumber { get; }
    }
}