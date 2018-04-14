namespace Contoso.Domain
{
    using Contoso.Domain.Tokens.Ordering;
    using More.Domain.Messaging;
    using System;

    public class OrderScenario : IScenario
    {
        readonly ScenarioBuilder builder;
        Guid aggregateId = Any.Guid;
        string billingAccountId = Any.NumericString;
        string catalogId = Any.NumericString;
        int quantity = 1;
        bool activateImmediately = false;
        string idempotencyToken = Any.IdempotencyToken;
        string correlationId = Any.CorrelationId;

        public OrderScenario( ScenarioBuilder builder ) => this.builder = builder;

        public OrderScenario HavingId( Guid value )
        {
            aggregateId = value;
            return this;
        }

        public OrderScenario WithBillingAccount( string value )
        {
            billingAccountId = value;
            return this;
        }

        public OrderScenario WithCatalogItem( string value )
        {
            catalogId = value;
            return this;
        }

        public OrderScenario WithQuantity( int value )
        {
            quantity = value;
            return this;
        }

        public OrderScenario IncludingTokenActivation()
        {
            activateImmediately = true;
            return this;
        }

        public OrderScenario UsingCorrelation( string value )
        {
            correlationId = value;
            return this;
        }

        public OrderScenario UsingIdempotencyToken( string value )
        {
            idempotencyToken = value;
            return this;
        }

        IMessage IScenario.Create()
        {
            return new PlaceOrder(
                aggregateId,
                billingAccountId,
                catalogId,
                quantity,
                activateImmediately,
                idempotencyToken,
                correlationId );
        }

        public ScenarioBuilder Then() => builder;
    }
}