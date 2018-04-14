namespace More.Domain.Options
{
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class OptionsBaseTest
    {
        [Fact]
        public void get_should_return_added_option()
        {
            // arrange
            var options = new AnyOptions();

            options.Add( new TestOption() );

            // act
            var option = options.Get<TestOption>();

            // assert
            option.Should().NotBeNull();
        }

        [Fact]
        public void get_should_throw_exception_when_option_is_not_present()
        {
            // arrange
            var options = new AnyOptions();

            // act
            Action get = () => options.Get<TestOption>();

            // assert
            get.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void try_get_should_return_added_option()
        {
            // arrange
            var options = new AnyOptions();

            options.Add( new TestOption() );

            // act
            options.TryGet( out TestOption option ).Should().BeTrue();

            // assert
            option.Should().NotBeNull();
        }

        [Fact]
        public void try_get_should_handle_missing_option()
        {
            // arrange
            var options = new AnyOptions();

            // act
            options.TryGet( out TestOption option ).Should().BeFalse();

            // assert
            option.Should().BeNull();
        }

        [Fact]
        public void all_should_return_expected_options()
        {
            // arrange
            var options = new AnyOptions();

            options.Add( new TestOption() );

            // act
            var all = options.All<TestOption>();

            // assert
            all.Should().ContainItemsAssignableTo<TestOption>();
        }

        class AnyOptions : OptionsBase { }

        class TestOption { }
    }
}