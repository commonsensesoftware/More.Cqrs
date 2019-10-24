namespace More.Domain.Commands
{
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class CommandRegistryTest
    {
        [Fact]
        public void register_should_add_activator_for_command_handler()
        {
            // arrange
            var registry = new CommandRegistrar();
            var test = new Test();

            // act
            registry.Register( () => new Handler() );

            // assert
            registry.ResolveFor( test ).Should().BeAssignableTo<Handler>();
        }

        [Fact]
        public void resolve_for_command_should_return_defined_handlers()
        {
            // arrange
            var services = new Dictionary<Type, object>() { [typeof( IEnumerable<IHandleCommand<Test>> )] = new[] { new Handler() } };
            var registry = new CommandRegistrar( new ServiceProvider( services ) );
            var doWork = new Test();

            // act
            var handlers = registry.ResolveFor( doWork );

            // assert
            handlers.Should().BeAssignableTo<Handler>();
        }

        class Test : Command { }

        class Handler : IHandleCommand<Test>
        {
            public ValueTask Handle( Test command, IMessageContext context, CancellationToken cancellationToken ) => default;
        }

        class ServiceProvider : IServiceProvider
        {
            readonly IReadOnlyDictionary<Type, object> services;

            public ServiceProvider( IReadOnlyDictionary<Type, object> services ) => this.services = services;

            public object GetService( Type serviceType ) => services[serviceType];
        }
    }
}