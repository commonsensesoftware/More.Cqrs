namespace Contoso.Domain
{
    using Contoso.Domain.Tokens.Minting;
    using More.Domain.Messaging;
    using System;

    public class MintScenario : IScenario
    {
        readonly ScenarioBuilder builder;
        Guid aggregateId = Any.Guid;
        string billingAccountId = Any.NumericString;
        string catalogId = Any.NumericString;
        long quantity = 1L;
        string idempotencyToken = Any.IdempotencyToken;
        string correlationId = Any.CorrelationId;

        public MintScenario( ScenarioBuilder builder ) => this.builder = builder;

        public MintScenario HavingId( Guid value )
        {
            aggregateId = value;
            return this;
        }

        public MintScenario WithBillingAccount( string value )
        {
            billingAccountId = value;
            return this;
        }

        public MintScenario WithCatalogItem( string value )
        {
            catalogId = value;
            return this;
        }

        public MintScenario WithQuantity( long value )
        {
            quantity = value;
            return this;
        }

        public MintScenario UsingCorrelation( string value )
        {
            correlationId = value;
            return this;
        }

        public MintScenario UsingIdempotencyToken( string value )
        {
            idempotencyToken = value;
            return this;
        }

        IMessage IScenario.Create()
        {
            return new Mint(
                aggregateId,
                billingAccountId,
                catalogId,
                quantity,
                idempotencyToken,
                correlationId );
        }

        public ScenarioBuilder Then() => builder;
    }
}