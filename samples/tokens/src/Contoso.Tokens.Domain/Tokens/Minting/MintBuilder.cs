namespace Contoso.Domain.Tokens.Minting
{
    using System;
    using static More.Domain.Uuid;

    public class MintBuilder
    {
        string billingAccountId;
        string catalogId;
        long quantity;
        string correlationId;
        string idempotencyToken;

        public MintBuilder ForBillingAccount( string value )
        {
            billingAccountId = value;
            return this;
        }

        public MintBuilder ForCatalogItem( string value )
        {
            catalogId = value;
            return this;
        }

        public MintBuilder WithQuantity( long value )
        {
            quantity = value;
            return this;
        }

        public MintBuilder CorrelatedBy( string value )
        {
            correlationId = value;
            return this;
        }

        public MintBuilder HasIdempotencyToken( string value )
        {
            idempotencyToken = value;
            return this;
        }

        public Mint NewMint()
        {
            return new Mint(
                NewSequentialId(),
                billingAccountId,
                catalogId,
                quantity,
                idempotencyToken,
                correlationId );
        }
    }
}