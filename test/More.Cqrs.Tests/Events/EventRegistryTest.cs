namespace More.Domain.Events
{
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class EventRegistryTest
    {
        [Fact]
        public void register_should_add_activator_for_event_receiver()
        {
            // arrange
            var registry = new EventRegistrar();
            var tested = new Tested();

            // act
            registry.Register( () => new Receiver() );

            // assert
            registry.ResolveFor( tested ).Should().ContainItemsAssignableTo<Receiver>();
        }

        [Fact]
        public void resolve_for_event_should_return_defined_receivers()
        {
            // arrange
            var services = new Dictionary<Type, object>() { [typeof( IEnumerable<IReceiveEvent<Tested>> )] = new[] { new Receiver() } };
            var registry = new EventRegistrar( new ServiceProvider( services ) );
            var tested = new Tested();

            // act
            var receivers = registry.ResolveFor( tested );

            // assert
            receivers.Should().ContainItemsAssignableTo<Receiver>();
        }

        class Tested : Event { }

        class Receiver : IReceiveEvent<Tested>
        {
            public ValueTask Receive( Tested @event, IMessageContext context, CancellationToken cancellationToken ) => default;
        }

        class ServiceProvider : IServiceProvider
        {
            readonly IReadOnlyDictionary<Type, object> services;

            public ServiceProvider( IReadOnlyDictionary<Type, object> services ) => this.services = services;

            public object GetService( Type serviceType ) => services[serviceType];
        }
    }
}