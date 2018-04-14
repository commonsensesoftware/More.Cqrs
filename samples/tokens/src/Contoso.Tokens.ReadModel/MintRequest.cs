namespace Contoso.Tokens
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class MintRequest
    {
        public Guid Id { get; set; }

        public int Version { get; set; }

        [Required]
        public string CatalogId { get; set; }

        [Range( typeof( long ), "1", "1099511627776" )]
        public long Quantity { get; set; }

        public MintingState State { get; set; }
    }
}