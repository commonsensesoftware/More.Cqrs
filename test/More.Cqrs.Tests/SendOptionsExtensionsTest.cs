namespace More.Domain
{
    using FluentAssertions;
    using More.Domain.Options;
    using System;
    using Xunit;

    public class SendOptionsExtensionsTest
    {
        [Fact]
        public void do_not_deliver_before_should_add_expected_send_option()
        {
            // arrange
            var options = new SendOptions();
            var tomorrow = DateTimeOffset.UtcNow.AddDays( 1d );

            // act
            options.DoNotDeliverBefore( tomorrow );

            // assert
            options.Get<DoNotDeliverBefore>().When.Should().Be( tomorrow.UtcDateTime );
        }

        [Fact]
        public void delay_delivery_by_should_add_expected_send_option()
        {
            // arrange
            var options = new SendOptions();
            var twelveHours = TimeSpan.FromHours( 12d );

            // act
            options.DelayDeliveryBy( twelveHours );

            // assert
            options.Get<DelayDeliveryBy>().Delay.Should().Be( twelveHours );
        }
    }
}