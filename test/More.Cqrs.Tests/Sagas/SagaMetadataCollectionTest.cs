namespace More.Domain.Sagas
{
    using Example;
    using FluentAssertions;
    using Xunit;

    public class SagaMetadataCollectionTest
    {
        SagaMetadataCollection Metadata { get; } = new SagaMetadataCollection( new[] { typeof( Marriage ) } );

        [Fact]
        public void saga_metadata_should_have_expected_count()
        {
            // arrange
            var count = 1;

            // act
            var result = Metadata.Count;

            // assert
            count.Should().Be( result );
        }

        [Fact]
        public void find_should_locate_metadata_by_saga_type()
        {
            // arrange
            var sagaType = typeof( Marriage );

            // act
            var result = Metadata.Find( sagaType );

            // assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void find_should_locate_metadata_by_saga_data_type()
        {
            // arrange
            var sagaDataType = typeof( MarriageData );

            // act
            var result = Metadata.FindByData( sagaDataType );

            // assert
            result.Should().NotBeNull();
        }
    }
}