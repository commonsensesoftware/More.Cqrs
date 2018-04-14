namespace Contoso.Services.V1
{
    using System;

    public class MintRequestFixture
    {
        public Guid MintRequestId { get; set; }

        public string CatalogId { get; set; }

        public long Quantity { get; set; }
    }
}