namespace Contoso.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using static System.Int32;

    public class Order
    {
        public Guid Id { get; set; }

        public int Version { get; set; }

        [Required]
        public string CatalogId { get; set; }

        public string BillingAccountId { get; set; }

        [Range( 1, MaxValue )]
        public int Quantity { get; set; }

        public OrderState State { get; set; }

        public IList<Token> Tokens { get; set; } = new List<Token>();
    }
}