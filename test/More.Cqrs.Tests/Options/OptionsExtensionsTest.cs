namespace More.Domain.Options
{
    using FluentAssertions;
    using System;
    using Xunit;
    using static System.DateTimeOffset;
    using static System.TimeSpan;

    public class OptionsExtensionsTest
    {
        [Fact]
        public void get_delivery_delay_should_return_explicit_delay()
        {
            // arrange
            var options = new SendOptions().DelayDeliveryBy( FromHours( 1d ) );

            // act
            var delay = options.GetDeliveryDelay();

            // assert
            delay.Should().Be( FromHours( 1d ) );
        }

        [Fact]
        public void get_delivery_delay_should_return_when_minus_now()
        {
            // arrange
            var tomorrow = UtcNow.AddDays( 1d );
            var options = new SendOptions().DoNotDeliverBefore( tomorrow );

            // act
            var delay = options.GetDeliveryDelay();

            // assert
            delay.Should().BeLessOrEqualTo( FromDays( 1d ) );
        }

        [Fact]
        public void get_delivery_delay_should_return_when_minus_clock()
        {
            // arrange
            var clock = new Clock( UtcNow );
            var tomorrow = clock.Now.AddDays( 1d );
            var options = new SendOptions().DoNotDeliverBefore( tomorrow );

            // act
            var delay = options.GetDeliveryDelay( clock );

            // assert
            delay.Should().Be( FromDays( 1d ) );
        }

        [Fact]
        public void get_delivery_time_should_return_when()
        {
            // arrange
            var tomorrow = UtcNow.AddDays( 1d );
            var options = new SendOptions().DoNotDeliverBefore( tomorrow );

            // act
            var deliveryTime = options.GetDeliveryTime();

            // assert
            deliveryTime.Should().Be( tomorrow );
        }

        [Fact]
        public void get_delivery_time_should_return_now_plus_delay()
        {
            // arrange
            var when = UtcNow.AddHours( 1d );
            var options = new SendOptions().DelayDeliveryBy( FromHours( 1d ) );

            // act
            var deliveryTime = options.GetDeliveryTime();

            // assert
            deliveryTime.Should().BeOnOrAfter( when );
        }

        [Fact]
        public void get_delivery_time_should_return_clock_plus_delay()
        {
            // arrange
            var clock = new Clock( UtcNow );
            var options = new SendOptions().DelayDeliveryBy( FromHours( 1d ) );

            // act
            var deliveryTime = options.GetDeliveryTime( clock );

            // assert
            deliveryTime.Should().Be( clock.Now.AddHours( 1d ) );
        }

        class Clock : IClock
        {
            internal Clock( DateTimeOffset time ) => Now = time;

            public DateTimeOffset Now { get; }
        }
    }
}