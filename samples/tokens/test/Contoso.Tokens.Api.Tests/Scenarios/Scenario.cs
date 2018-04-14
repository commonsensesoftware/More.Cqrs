namespace Contoso.Services.Scenarios
{
    using More.Domain.Sagas;
    using System;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public abstract class Scenario : IScenario
    {
        readonly ScenarioBuilder builder;
        readonly ITestOutputHelper output;

        protected Scenario( ScenarioBuilder builder, ApiTest api, ITestOutputHelper output )
        {
            this.builder = builder;
            Api = api;
            this.output = output;
        }

        protected ScenarioData Data => builder.Data;

        protected ApiTest Api { get; }

        public ScenarioBuilder Then() => builder;

        protected void WriteLine( string message ) => output.WriteLine( message );

        protected void WriteLine( string format, params object[] args ) => output.WriteLine( format, args );

        protected SagaFinder<TData> Saga<TData>() where TData : class, ISagaData => new SagaFinder<TData>();

        protected abstract Task Run();

        Func<Task> IScenario.Create() => Run;
    }
}