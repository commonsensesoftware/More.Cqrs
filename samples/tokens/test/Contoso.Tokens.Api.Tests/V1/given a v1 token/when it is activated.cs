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

    public class when_it_is_activated : AcceptanceTest
    {
        public when_it_is_activated( ITestOutputHelper output ) : base( output )
        {
            Setup
                 .Use().PartnerId( "bob@northwind.com" ).CatalogId( Any.CatalogId )
                 .Then().Mint().WithQuantity( 1L )
                 .Then().PlaceOrder().WithQuantity( 1 )
                 .Then().Retrieve().NextToken().WithState( Activated );
        }

        [Fact]
        public async Task then_it_should_be_redeemable()
        {
            // arrange
            var token = Setup.Data.Tokens.Single();

            Client.DefaultRequestHeaders.From = "john.doe@live.com";
            Client.DefaultRequestHeaders.IfMatch.ParseAdd( token.ETag );

            // act
            var response = await PostAsync( $"tokens/redeem", new { code = token.Code } );

            await Aggregate.Token.WithId( token.Id ).ToRecordEvent<TokenRedeemed>();

            // assert
            response.StatusCode.Should().Be( OK );
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

        [Fact]
        public async Task then_it_can_be_deactivated()
        {
            // arrange
            var token = Setup.Data.Tokens.Single();

            Client.DefaultRequestHeaders.IfMatch.ParseAdd( token.ETag );

            // act
            var response = await PostAsync( $"tokens('{token.Id}')/deactivate" );

            await Aggregate.Token.WithId( token.Id ).ToRecordEvent<TokenDeactivated>();

            // assert
            response.StatusCode.Should().Be( OK );
        }

        [Fact]
        public async Task then_it_cannot_be_activated_for_a_different_account()
        {
            // arrange
            var token = Setup.Data.Tokens.Single();

            Client.DefaultRequestHeaders.From = "john@fabrikam.com";
            Client.DefaultRequestHeaders.IfMatch.ParseAdd( token.ETag );

            // act
            var response = await PostAsync( $"tokens('{token.Id}')/activate" );

            // assert
            response.StatusCode.Should().Be( Conflict );
        }
    }
}