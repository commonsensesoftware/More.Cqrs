namespace Contoso.Domain.Tokens.Minting
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay( "Id = {Id}, CatalogId = {CatalogId}, Start = {StartOfSequence}, Count = {Count}" )]
    public class MintJob
    {
        public MintJob( int id, string catalogId, long startOfSequence, long count )
        {
            Id = id;
            CatalogId = catalogId;
            StartOfSequence = startOfSequence;
            Count = count;
        }

        public int Id { get; }

        public string CatalogId { get; }

        public long StartOfSequence { get; }

        public long Count { get; }
    }
}