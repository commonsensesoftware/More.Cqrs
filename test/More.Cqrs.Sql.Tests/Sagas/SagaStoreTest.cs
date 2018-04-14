namespace More.Domain.Sagas
{
    using FluentAssertions;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using static More.Domain.Sagas.SagaMetadata;
    using static System.Guid;

    public class SagaStoreTest : DatabaseTest<SagaFixture>
    {
        public SagaStoreTest( SagaFixture setup ) : base( setup ) => Sagas = new SqlSagaStorage( setup.Configuration );

        SqlSagaStorage Sagas { get; }

        SagaMetadata Metadata { get; } = Create( typeof( Procurement ) );

        [Fact]
        public async Task retrieve_should_load_stored_saga()
        {
            // arrange
            var storedData = new ProcurementData() { Id = NewGuid(), OrderId = NewGuid() };
            var correlationProperty = new CorrelationProperty( Metadata.CorrelationProperty, storedData.OrderId, isDefaultValue: false );

            await Sagas.Store( storedData, correlationProperty, CancellationToken.None );

            // act
            var retrievedData = await Sagas.Retrieve<ProcurementData>( storedData.Id, CancellationToken.None );

            // assert
            retrievedData.Should().BeEquivalentTo( storedData );
        }

        [Fact]
        public async Task retrieve_should_load_saga_by_correlated_property()
        {
            // arrange
            var storedData = new ProcurementData() { Id = NewGuid(),  OrderId = NewGuid() };
            var correlationProperty = new CorrelationProperty( Metadata.CorrelationProperty, storedData.OrderId, isDefaultValue: false );

            await Sagas.Store( storedData, correlationProperty, CancellationToken.None );

            // act
            var retrievedData = await Sagas.Retrieve<ProcurementData>( nameof( ProcurementData.OrderId ), storedData.OrderId, CancellationToken.None );

            // assert
            retrievedData.Should().BeEquivalentTo( storedData );
        }

        [Fact]
        public async Task retrieve_should_not_return_a_saga_marked_completed()
        {
            // arrange
            var storedData = new ProcurementData() { Id = NewGuid(), OrderId = NewGuid() };
            var correlationProperty = new CorrelationProperty( Metadata.CorrelationProperty, storedData.OrderId, isDefaultValue: false );

            await Sagas.Store( storedData, correlationProperty, CancellationToken.None );

            // act
            await Sagas.Complete( storedData, CancellationToken.None );
            var retrievedData = await Sagas.Retrieve<ProcurementData>( storedData.Id, CancellationToken.None );

            // assert
            retrievedData.Should().BeNull();
        }
    }
}