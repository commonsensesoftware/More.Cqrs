namespace Contoso.Services.Composition.Conventions
{
    using More.ComponentModel;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web.Http.Controllers;

    sealed class ControllerConvention : IRule<ConventionContext>
    {
        readonly IRule<IEnumerable<ConstructorInfo>, ConstructorInfo> SelectConstructor = new ConstructorSelectionRule();

        public void Evaluate( ConventionContext context )
        {
            var builder = context.Conventions.ForTypesDerivedFrom<IHttpController>().Export();

            // TODO: apply conventions to decorate components for logging

            //var constructorRule = new ConstructorSelectionRule();
            //var importParameterRule = new DecoratedParameterRule( typeof( ICommandSender ), typeof( IReadOnlyRepository<> ), typeof( IAccountFinder ) );

            //builder.SelectConstructor( constructorRule.Evaluate, ( p, b ) => importParameterRule.Evaluate( new ImportParameter( p, b ) ) );
        }
    }
}