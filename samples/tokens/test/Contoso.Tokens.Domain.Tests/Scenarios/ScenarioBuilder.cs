namespace Contoso.Domain
{
    using More.Domain.Commands;
    using More.Domain.Events;
    using More.Domain.Messaging;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class ScenarioBuilder
    {
        readonly MessageBus bus;
        readonly List<IScenario> scenarios = new List<IScenario>();

        public ScenarioBuilder( MessageBus bus ) => this.bus = bus;

        public MintScenario Mint()
        {
            var scenario = new MintScenario( this );
            scenarios.Add( scenario );
            return scenario;
        }

        public OrderScenario PlaceOrder()
        {
            var scenario = new OrderScenario( this );
            scenarios.Add( scenario );
            return scenario;
        }

        public async Task Run()
        {
            var messages = scenarios.Select( s => s.Create() );

            foreach ( var message in messages )
            {
                switch ( message )
                {
                    case ICommand command:
                        await bus.Send( command );
                        await bus.Flush();
                        break;
                    case IEvent @event:
                        await bus.Publish( @event );
                        await bus.Flush();
                        break;
                }
            }
        }
    }
}