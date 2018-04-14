namespace Contoso.Tokens
{
    using System;

    public class Token
    {
        public string Id { get; set; }

        public int Version { get; set; }

        public string CatalogId { get; set; }

        public string ReservedByBillingAccountId { get; set; }

        public string RedeemedByAccountId { get; set; }

        public string Code { get; set; }

        public string Hash { get; set; }

        public TokenState State { get; set; }
    }
}