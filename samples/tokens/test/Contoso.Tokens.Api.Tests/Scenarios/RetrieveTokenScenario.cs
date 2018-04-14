namespace Contoso.Services.Scenarios
{
    using Contoso.Tokens;
    using System;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public class RetrieveTokenScenario : Scenario
    {
        int quantity;
        TokenState state;

        public RetrieveTokenScenario( ScenarioBuilder builder, ApiTest api, ITestOutputHelper output )
            : base( builder, api, output ) { }

        public RetrieveTokenScenario NextToken() => NextTokens( 1 );

        public RetrieveTokenScenario NextTokens( int value )
        {
            quantity = value;
            return this;
        }

        public RetrieveTokenScenario WithState( TokenState value )
        {
            state = value;
            return this;
        }

        protected override async Task Run()
        {
            if ( Data.OrderId == default( Guid ) )
            {
                throw new InvalidOperationException( "A token cannot be retrieved before an order has been placed." );
            }

            if ( quantity < 1 )
            {
                throw new InvalidOperationException( "The retrieve token scenario requires at least one token." );
            }

            WriteLine( $"Starting retrieve ordered tokens scenario (state = {state}, quantity = {quantity})" );

            var example = new { value = new[] { new { id = "", code = "", odata_etag = "" } } };
            var response = await Api.GetAsync( $"orders({Data.OrderId})/tokens?$top={quantity}&$filter=state eq '{state}'&$select=id,code,hash" );
            var tokens = ( await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example ) ).value;

            WriteLine( $"  tokens (count = {tokens.Length}):" );

            foreach ( var token in tokens )
            {
                WriteLine( "    " + token.id );
                Data.Tokens.Add( new RetrievedToken() { Id = token.id, Code = token.code, ETag = token.odata_etag } );
            }
        }
    }
}