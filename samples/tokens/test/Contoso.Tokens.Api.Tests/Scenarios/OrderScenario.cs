namespace Contoso.Services.Scenarios
{
    using Contoso.Domain.Tokens.Ordering;
    using System;
    using System.Threading.Tasks;
    using Xunit.Abstractions;
    using static System.String;

    public class OrderScenario : Scenario
    {
        string partnerId;
        string catalogId;
        int quantity;
        bool reserveOnly;

        public OrderScenario( ScenarioBuilder builder, ApiTest api, ITestOutputHelper output )
            : base( builder, api, output )
        {
            partnerId = builder.Data.PartnerId;
            catalogId = builder.Data.CatalogId;
        }

        public OrderScenario ForPartner( string value )
        {
            partnerId = value;
            return this;
        }

        public OrderScenario WithCatalogId( string value )
        {
            catalogId = value;
            return this;
        }

        public OrderScenario WithQuantity( int value )
        {
            quantity = value;
            return this;
        }

        public OrderScenario SkipTokenActivation()
        {
            reserveOnly = true;
            return this;
        }

        protected override async Task Run()
        {
            if ( IsNullOrEmpty( catalogId ) )
            {
                throw new InvalidOperationException( "The order scenario requires a catalog identifier." );
            }

            if ( quantity < 1 )
            {
                throw new InvalidOperationException( "The order scenario requires at least one token." );
            }

            WriteLine( $"Starting order scenario (partner = {partnerId}, reserve only = {reserveOnly.ToString().ToLower()})" );

            if ( !IsNullOrEmpty( partnerId ) )
            {
                Api.AddHeader( "from", partnerId );
            }

            var requestUri = "orders";

            if ( reserveOnly )
            {
                requestUri += "?reserveOnly=true";
            }

            var response = await Api.PostAsync( requestUri, new { catalogId = catalogId, quantity = quantity } );
            var id = Data.OrderId = response.EnsureSuccessStatusCode().IdFromLocationHeader<Guid>();

            await Saga<OrderData>().Where( s => s.Id == id ).ToComplete();
            WriteLine( $"Completed order scenario (order id = {id})" );
        }
    }
}