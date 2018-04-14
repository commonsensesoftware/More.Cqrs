namespace Contoso.Services.Scenarios
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public class ScenarioBuilder
    {
        readonly List<IScenario> scenarios = new List<IScenario>();
        readonly ApiTest api;
        readonly ITestOutputHelper output;

        public ScenarioBuilder( ApiTest api, ITestOutputHelper output )
        {
            this.api = api;
            this.output = output;
        }

        public ScenarioData Data { get; } = new ScenarioData();

        public ScenarioBuilderConfiguration Use() => new ScenarioBuilderConfiguration( this );

        public MintScenario Mint()
        {
            var scenario = new MintScenario( this, api, output );
            scenarios.Add( scenario );
            return scenario;
        }

        public OrderScenario PlaceOrder()
        {
            var scenario = new OrderScenario( this, api, output );
            scenarios.Add( scenario );
            return scenario;
        }

        public RetrieveTokenScenario Retrieve()
        {
            var scenario = new RetrieveTokenScenario( this, api, output );
            scenarios.Add( scenario );
            return scenario;
        }

        public async Task Run()
        {
            Data.ClientRequestId = api.AddClientRequestId();

            foreach ( var scenario in scenarios )
            {
                await scenario.Create()();
            }
        }
    }
}