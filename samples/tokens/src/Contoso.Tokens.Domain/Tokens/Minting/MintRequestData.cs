namespace Contoso.Domain.Tokens.Minting
{
    using More.Domain.Sagas;
    using System;

    public class MintRequestData : SagaData
    {
        public MintingState State { get; set; }

        public string CatalogId { get; set; }

        public bool PartiallyCanceled { get; set; }

        public string IdempotencyToken { get; set; }

        public KeyedCollection<int, MintJob> MintJobs { get; } = new KeyedCollection<int, MintJob>( job => job.Id );
    }
}