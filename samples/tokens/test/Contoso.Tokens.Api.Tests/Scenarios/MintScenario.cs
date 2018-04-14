namespace Contoso.Services.Scenarios
{
    using Contoso.Domain.Tokens.Minting;
    using System;
    using System.Threading.Tasks;
    using Xunit.Abstractions;
    using static System.String;

    public class MintScenario : Scenario
    {
        string partnerId;
        string catalogId;
        long quantity;

        public MintScenario( ScenarioBuilder builder, ApiTest api, ITestOutputHelper output )
            : base( builder, api, output )
        {
            partnerId = builder.Data.PartnerId;
            catalogId = builder.Data.CatalogId;
        }

        public MintScenario ForPartner( string value )
        {
            partnerId = value;
            return this;
        }

        public MintScenario WithCatalogId( string value )
        {
            catalogId = value;
            return this;
        }

        public MintScenario WithQuantity( long value )
        {
            quantity = value;
            return this;
        }

        protected override async Task Run()
        {
            if ( IsNullOrEmpty( catalogId ) )
            {
                throw new InvalidOperationException( "The mint scenario requires a catalog identifier for minting." );
            }

            if ( quantity < 1L )
            {
                throw new InvalidOperationException( "The mint scenario requires at least one token to be minted." );
            }

            WriteLine( $"Starting mint scenario (partner = {partnerId})" );

            if ( !IsNullOrEmpty( partnerId ) )
            {
                Api.AddHeader( "from", partnerId );
            }

            var response = await Api.PostAsync( "mintrequests", new { catalogId = catalogId, quantity = quantity } );
            var id = Data.MintRequestId = response.EnsureSuccessStatusCode().IdFromLocationHeader<Guid>();

            await Saga<MintRequestData>().Where( s => s.Id == id ).ToComplete();
            WriteLine( $"Completed mint scenario (mint request id = {id})" );
        }
    }
}