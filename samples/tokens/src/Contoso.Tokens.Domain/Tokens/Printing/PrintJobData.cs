namespace Contoso.Domain.Tokens.Printing
{
    using More.Domain.Sagas;
    using System.Collections.Generic;

    public class PrintJobData : SagaData
    {
        public PrintingState State { get; set; }

        public string BillingAccountId;

        public string CatalogId;

        public long QuantityRequested;

        public string CertificateThumbprint;

        public string IdempotencyToken { get; set; }

        public KeyedCollection<string, TokenReference> SpooledTokens { get; } =
            new KeyedCollection<string, TokenReference>( token => token.Id );

        public HashSet<string> PrintedTokens { get; } = new HashSet<string>();
    }
}