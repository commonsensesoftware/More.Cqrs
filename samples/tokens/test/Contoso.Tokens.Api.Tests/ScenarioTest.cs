namespace Contoso.Services
{
    using Contoso.Services.Scenarios;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;
    using static System.Threading.Tasks.Task;

    public abstract class ScenarioTest : ApiTest, IAsyncLifetime
    {
        protected ScenarioTest( ITestOutputHelper output )
            : base( output ) => Setup = new ScenarioBuilder( this, output );

        protected ScenarioBuilder Setup { get; }

        public virtual Task InitializeAsync() => Setup.Run();

        public virtual Task DisposeAsync() => CompletedTask;
    }
}