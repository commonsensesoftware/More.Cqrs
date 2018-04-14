namespace given_a_mint_request
{
    using Contoso.Domain;
    using Contoso.Domain.Tokens.Minting;
    using FluentAssertions;
    using More.Domain;
    using More.Domain.Commands;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;
    using static Contoso.Tokens.MintingState;

    public class when_it_completes_successfully : ScenarioTest
    {
        public when_it_completes_successfully( ITestOutputHelper output ) : base( output ) { }

        [Fact]
        public async Task then_the_request_should_be_in_the_completed_state()
        {
            // arrange
            var mint = new Mint(
                aggregateId: Any.Guid,
                billingAccountId: Any.NumericString,
                catalogId: Any.NumericString,
                quantity: 10L,
                idempotencyToken: Any.IdempotencyToken,
                correlationId: ScenarioCorrelationId );

            // act
            await Bus.Send( mint );
            await Bus.Flush();

            // assert
            MintRequests.Single().Should().BeEquivalentTo(
                new
                {
                    Id = mint.AggregateId,
                    Version = 0,
                    CatalogId = mint.CatalogId,
                    Quantity = mint.Quantity,
                    State = Completed
                } );
        }

        [Fact]
        public async Task then_the_vault_should_contain_the_expected_tokens()
        {
            // arrange
            var mint = new Mint(
                aggregateId: Any.Guid,
                billingAccountId: Any.NumericString,
                catalogId: Any.NumericString,
                quantity: 10L,
                idempotencyToken: Any.IdempotencyToken,
                correlationId: ScenarioCorrelationId );

            // act
            await Bus.Send( mint );
            await Bus.Flush();

            await Task.Delay( 2000 );
            await Bus.Flush();

            // assert
            Tokens.Should().HaveCount( 10 );
        }
    }
}