namespace given_an_order
{
    using Contoso.Domain;
    using Contoso.Domain.Tokens.Ordering;
    using FluentAssertions;
    using More.Domain;
    using More.Domain.Commands;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;
    using static Contoso.Tokens.OrderState;
    using static Contoso.Tokens.TokenState;
    using static System.TimeSpan;

    public class when_it_times_out : ScenarioTest
    {
        static readonly TimeSpan OneSecond = FromSeconds( 1d );
        static readonly TimeSpan OneDay = FromDays( 1.1 );

        public when_it_times_out( ITestOutputHelper output ) : base( output )
        {
            Messages.Delay<OrderPlaced>().By( OneSecond );
            Setup.Mint().WithBillingAccount( BillingAccountId )
                        .WithCatalogItem( CatalogId )
                        .WithQuantity( 10L )
                        .UsingCorrelation( ScenarioCorrelationId );
        }

        string BillingAccountId { get; } = Any.NumericString;

        string CatalogId { get; } = Any.NumericString;

        [Fact]
        public async Task then_all_reserved_tokens_should_be_deactivated()
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

            Clock.AdvanceBy( OneDay );

            // act
            await Bus.Send( placeOrder );
            await Bus.Flush();

            // assert
            Orders.Single().Should().BeEquivalentTo(
                new
                {
                    Id = placeOrder.AggregateId,
                    Version = 1,
                    CatalogId = placeOrder.CatalogId,
                    BillingAccountId = placeOrder.BillingAccountId,
                    Quantity = placeOrder.Quantity,
                    State = Canceled,
                    Tokens = new List<string>()
                } );
            Tokens.Should().OnlyContain( token => token.State == Circulated );
        }

        [Fact]
        public async Task then_all_activated_tokens_should_be_deactivated()
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

            Clock.AdvanceBy( OneDay );

            // act
            await Bus.Send( placeOrder );
            await Bus.Flush();

            // assert
            Orders.Single().Should().BeEquivalentTo(
                new
                {
                    Id = placeOrder.AggregateId,
                    Version = 1,
                    CatalogId = placeOrder.CatalogId,
                    BillingAccountId = placeOrder.BillingAccountId,
                    Quantity = placeOrder.Quantity,
                    State = Canceled,
                    Tokens = new List<string>()
                } );
            Tokens.Should().OnlyContain( token => token.State == Circulated );
        }
    }
}