namespace given_a_v1_mint_request
{
    using Contoso.Domain.Tokens.Minting;
    using Contoso.Services;
    using Contoso.Services.V1;
    using FluentAssertions;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;
    using static Contoso.Tokens.MintingState;
    using static System.Net.HttpStatusCode;
    using MintingState = Contoso.Tokens.MintingState;

    [TestCaseOrderer( "Contoso.Services.TestSequence", "Contoso.Tokens.Api.Tests" )]
    public class when_a_valid_post_is_sent : AcceptanceTest, IClassFixture<MintRequestFixture>
    {
        public when_a_valid_post_is_sent( ITestOutputHelper output, MintRequestFixture fixture ) : base( output )
        {
            Test = fixture;
            Test.CatalogId = "12345";
            Test.Quantity = 10L;
        }

        MintRequestFixture Test { get; }

        [Fact, Step( 1 )]
        public async Task then_the_response_should_be_201()
        {
            // arrange
            var mintRequest = new { catalogId = Test.CatalogId, quantity = Test.Quantity };

            AddClientRequestId();

            // act
            var response = await PostAsync( "mintrequests", mintRequest );

            // assert
            response.StatusCode.Should().Be( Created );

            Test.MintRequestId = response.IdFromLocationHeader<Guid>();
            await Saga<MintRequestData>().Where( s => s.Id == Test.MintRequestId ).ToComplete();
        }

        [Fact, Step( 2 )]
        public async Task then_get_should_retrieve_the_created_mint_request()
        {
            // arrange
            var example = new { id = default( Guid ), version = 0, catalogId = "", quantity = 0L, state = default( MintingState ) };

            // act
            var response = await GetAsync( $"mintrequests({Test.MintRequestId})" );
            var mintRequest = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

            // assert
            mintRequest.Should().BeEquivalentTo(
                new
                {
                    id = Test.MintRequestId,
                    version = 1,
                    catalogId = Test.CatalogId,
                    quantity = Test.Quantity,
                    state = Completed
                } );
        }
    }
}