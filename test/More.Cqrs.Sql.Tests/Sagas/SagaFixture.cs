namespace More.Domain.Sagas
{
    using System.Threading.Tasks;
    using Xunit;
    using static System.Threading.Tasks.Task;

    public class SagaFixture : IAsyncLifetime
    {
        public SagaFixture( DatabaseFixture database )
        {
            var builder = new SqlSagaStorageConfigurationBuilder();

            Database = database;
            Configuration = builder.HasConnectionString( database.ConnectionString ).CreateConfiguration();
        }

        protected DatabaseFixture Database { get; }

        public SqlSagaStorageConfiguration Configuration { get; }

        public Task DisposeAsync() => CompletedTask;

        public Task InitializeAsync() => Configuration.CreateTables();
    }
}