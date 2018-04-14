namespace given_an_order
{
    using Contoso.Domain;
    using Contoso.Domain.Tokens.Ordering;
    using FluentAssertions;
    using More.Domain;
    using More.Domain.Commands;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;
    using static Contoso.Tokens.OrderState;
    using static Contoso.Tokens.TokenState;

    public class when_it_completes_successfully : ScenarioTest
    {
        public when_it_completes_successfully( ITestOutputHelper output ) : base( output )
        {
            Messages.Ignore<OrderTookTooLongToFulfill>();
            Setup.Mint().WithBillingAccount( BillingAccountId )
                        .WithCatalogItem( CatalogId )
                        .WithQuantity( 10L )
                        .UsingCorrelation( ScenarioCorrelationId );
        }

        string BillingAccountId { get; } = Any.NumericString;

        string CatalogId { get; } = Any.NumericString;

        [Fact]
        public async Task then_the_ordered_tokens_should_be_reserved()
        {
            // arrange
            var placeOrder = new PlaceOrder(
                aggregateId: Any.Guid,
                billingAccountId: BillingAccountId,
                catalogId: CatalogId,
                quantity: 5,
                activateImmediately: false,
                idempotencyToken: Any.IdempotencyToken,
                correlationId: ScenarioCorrelationId );

            // act
            await Bus.Send( placeOrder );
            await Bus.Flush();

            var order = Orders.Single();
            var tokens = from orderedToken in order.Tokens
                         from token in Tokens
                         where token.Id == orderedToken.Id
                         select token;

            // assert
            order.Should().BeEquivalentTo(
                new
                {
                    Id = placeOrder.AggregateId,
                    Version = 0,
                    CatalogId = placeOrder.CatalogId,
                    BillingAccountId = placeOrder.BillingAccountId,
                    Quantity = placeOrder.Quantity,
                    State = Fulfilled
                },
                options => options.ExcludingMissingMembers() );
            tokens.Should().OnlyContain( token => token.State == Reserved );
        }

        [Fact]
        public async Task then_the_ordered_tokens_should_be_activated()
        {
            // arrange
            var placeOrder = new PlaceOrder(
                aggregateId: Any.Guid,
                billingAccountId: BillingAccountId,
                catalogId: CatalogId,
                quantity: 5,
                activateImmediately: true,
                idempotencyToken: Any.IdempotencyToken,
                correlationId: ScenarioCorrelationId );

            // act
            await Bus.Send( placeOrder );
            await Bus.Flush();

            var order = Orders.Single();
            var tokens = ( from orderedToken in order.Tokens
                           from token in Tokens
                           where token.Id == orderedToken.Id
                           select token ).ToArray();

            // assert
            order.Should().BeEquivalentTo(
                new
                {
                    Id = placeOrder.AggregateId,
                    Version = 0,
                    CatalogId = placeOrder.CatalogId,
                    BillingAccountId = placeOrder.BillingAccountId,
                    Quantity = placeOrder.Quantity,
                    State = Fulfilled
                },
                options => options.ExcludingMissingMembers() );
            tokens.Take( 5 ).Should().OnlyContain( token => token.State == Activated );
        }
    }
}