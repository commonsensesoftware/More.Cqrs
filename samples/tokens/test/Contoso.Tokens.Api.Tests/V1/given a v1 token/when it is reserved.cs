namespace given_a_v1_token
{
    using Contoso.Domain.Tokens;
    using Contoso.Services;
    using Contoso.Services.V1;
    using FluentAssertions;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;
    using static Contoso.Tokens.TokenState;
    using static System.Net.HttpStatusCode;

    public class when_it_is_reserved : AcceptanceTest
    {
        public when_it_is_reserved( ITestOutputHelper output ) : base( output )
        {
            Setup
                 .Use().PartnerId( "bob@northwind.com" ).CatalogId( Any.CatalogId )
                 .Then().Mint().WithQuantity( 1L )
                 .Then().PlaceOrder().WithQuantity( 1 ).SkipTokenActivation()
                 .Then().Retrieve().NextToken().WithState( Reserved );
        }

        [Fact]
        public async Task then_it_can_be_activated()
        {
            // arrange
            var token = Setup.Data.Tokens.Single();

            Client.DefaultRequestHeaders.From = "bob@northwind.com";
            Client.DefaultRequestHeaders.IfMatch.ParseAdd( token.ETag );

            // act
            var response = await PostAsync( $"tokens('{token.Id}')/activate" );

            await Aggregate.Token.WithId( token.Id ).ToRecordEvent<TokenActivated>();

            // assert
            response.StatusCode.Should().Be( OK );
        }

        [Fact]
        public async Task then_it_should_not_be_redeemable_until_activated()
        {
            // arrange
            var token = Setup.Data.Tokens.Single();

            Client.DefaultRequestHeaders.From = "john.doe@live.com";
            Client.DefaultRequestHeaders.IfMatch.ParseAdd( token.ETag );

            // act
            var response = await PostAsync( $"tokens/redeem", new { code = token.Code } );

            // assert
            response.StatusCode.Should().Be( BadRequest );
        }

        [Fact]
        public async Task then_it_can_be_voided()
        {
            // arrange
            var token = Setup.Data.Tokens.Single();

            Client.DefaultRequestHeaders.IfMatch.ParseAdd( token.ETag );

            // act
            var response = await DeleteAsync( $"tokens('{token.Id}')" );

            await Aggregate.Token.WithId( token.Id ).ToRecordEvent<TokenVoided>();

            // assert
            response.StatusCode.Should().Be( NoContent );
        }
    }
}