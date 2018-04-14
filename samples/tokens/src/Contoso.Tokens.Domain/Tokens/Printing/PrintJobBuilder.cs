namespace Contoso.Domain.Tokens.Printing
{
    using System;
    using static More.Domain.Uuid;

    public class PrintJobBuilder
    {
        string billingAccountId;
        string catalogId;
        int quantity;
        string certificateThumbprint;
        string correlationId;
        string idempotencyToken;

        public PrintJobBuilder ForBillingAccount( string value )
        {
            billingAccountId = value;
            return this;
        }

        public PrintJobBuilder ForCatalogItem( string value )
        {
            catalogId = value;
            return this;
        }

        public PrintJobBuilder WithQuantity( int value )
        {
            quantity = value;
            return this;
        }

        public PrintJobBuilder WithCertificateThumbprint( string value )
        {
            certificateThumbprint = value;
            return this;
        }

        public PrintJobBuilder CorrelatedBy( string value )
        {
            correlationId = value;
            return this;
        }

        public PrintJobBuilder HasIdempotencyToken( string value )
        {
            idempotencyToken = value;
            return this;
        }

        public Print NewPrint()
        {
            return new Print(
                NewSequentialId(),
                billingAccountId,
                catalogId,
                quantity,
                certificateThumbprint,
                idempotencyToken,
                correlationId );
        }
    }
}
