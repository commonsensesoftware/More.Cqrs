namespace More.Domain.Messaging
{
    using System.Threading.Tasks;
    using Xunit;
    using static System.Threading.Tasks.Task;

    public class QueueFixture : IAsyncLifetime
    {
        public QueueFixture( DatabaseFixture database )
        {
            var builder = new SqlMessageQueueConfigurationBuilder();

            Database = database;
            Configuration = builder.HasConnectionString( database.ConnectionString ).CreateConfiguration();
        }

        protected DatabaseFixture Database { get; }

        public SqlMessageQueueConfiguration Configuration { get; }

        public Task DisposeAsync() => CompletedTask;

        public Task InitializeAsync() => Configuration.CreateTables();
    }
}