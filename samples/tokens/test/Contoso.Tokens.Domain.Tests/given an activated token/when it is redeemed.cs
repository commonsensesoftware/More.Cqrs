namespace given_an_activated_token
{
    using Contoso.Domain;
    using Contoso.Domain.Tokens;
    using Contoso.Domain.Tokens.Ordering;
    using FluentAssertions;
    using More.Domain;
    using More.Domain.Commands;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;
    using static Contoso.Tokens.TokenState;

    public class when_it_is_redeemed : ScenarioTest
    {
        public when_it_is_redeemed( ITestOutputHelper output ) : base( output )
        {
            var billingAccountId = Any.NumericString;
            var catalogId = Any.NumericString;

            Messages.Ignore<OrderTookTooLongToFulfill>();
            Setup
                 .Mint().WithBillingAccount( billingAccountId )
                        .WithCatalogItem( catalogId )
                        .UsingCorrelation( ScenarioCorrelationId )
                 .Then()
                 .PlaceOrder().WithBillingAccount( billingAccountId )
                              .WithCatalogItem( catalogId )
                              .IncludingTokenActivation()
                              .UsingCorrelation( ScenarioCorrelationId );
        }

        [Fact]
        public async Task then_the_token_should_transition_state()
        {
            // arrange
            var token = Tokens.First();
            var redeem = new RedeemToken( token.Id, token.Version, accountId: "bob@fabrikam.com" )
            {
                CorrelationId = ScenarioCorrelationId
            };

            // act
            await Bus.Send( redeem );
            await Bus.Flush();

            // assert
            token.State.Should().Be( Redeemed );
        }
    }
}