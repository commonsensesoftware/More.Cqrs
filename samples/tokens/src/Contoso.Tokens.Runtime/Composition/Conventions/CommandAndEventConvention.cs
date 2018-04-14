namespace Contoso.Services.Composition.Conventions
{
    using More.ComponentModel;
    using More.Domain.Commands;
    using More.Domain.Events;
    using System;

    sealed class CommandAndEventConvention : IRule<ConventionContext>
    {
        readonly ISpecification<Type> CommandOrEvent;
        readonly ISpecification<Type> CommandAndEvent;

        internal CommandAndEventConvention()
        {
            var type = typeof( IHandleCommand<> );

            CommandOrEvent = new InterfaceSpecification( type )
                        .Or( new InterfaceSpecification( typeof( IReceiveEvent<> ) ) );

            CommandAndEvent = new InterfaceSpecification( type, exactMatch: true )
                        .Or( new InterfaceSpecification( typeof( IReceiveEvent<> ), exactMatch: true ) );
        }

        public void Evaluate( ConventionContext context ) =>
            context.Conventions
                   .ForTypesMatching( CommandOrEvent.IsSatisfiedBy )
                   .ExportInterfaces( CommandAndEvent.IsSatisfiedBy );
    }
}