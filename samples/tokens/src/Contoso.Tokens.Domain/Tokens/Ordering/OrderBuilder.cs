namespace Contoso.Domain.Tokens.Ordering
{
    using System;
    using static More.Domain.Uuid;

    public class OrderBuilder
    {
        string billingAccountId;
        string catalogId;
        int quantity;
        bool reserveOnly;
        string correlationId;
        string idempotencyToken;

        public OrderBuilder ForBillingAccount( string value )
        {
            billingAccountId = value;
            return this;
        }

        public OrderBuilder ForCatalogItem( string value )
        {
            catalogId = value;
            return this;
        }

        public OrderBuilder WithQuantity( int value )
        {
            quantity = value;
            return this;
        }

        public OrderBuilder ReserveTokensOnly()
        {
            reserveOnly = true;
            return this;
        }

        public OrderBuilder CorrelatedBy( string value )
        {
            correlationId = value;
            return this;
        }

        public OrderBuilder HasIdempotencyToken( string value )
        {
            idempotencyToken = value;
            return this;
        }

        public PlaceOrder NewPlaceOrder()
        {
            return new PlaceOrder(
                NewSequentialId(),
                billingAccountId,
                catalogId,
                quantity,
                !reserveOnly,
                idempotencyToken,
                correlationId );
        }
    }
}
