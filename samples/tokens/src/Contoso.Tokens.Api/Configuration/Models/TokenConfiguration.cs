namespace Contoso.Services.Configuration.Models
{
    using Microsoft.Web.Http;
    using Microsoft.Web.OData.Builder;
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Web.OData;
    using System.Web.OData.Builder;
    using Tokens;
    using static Contoso.Tokens.TokenState;
    using static DefinedApiVersions;

    public class TokenConfiguration : IModelConfiguration
    {
        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion )
        {
            if ( apiVersion == V1 )
            {
                ApplyForVersion1( builder );
            }
        }

        static void ApplyForVersion1( ODataModelBuilder builder )
        {
            const bool followsConventions = false;
            var tokens = builder.EntitySet<Token>( "Tokens" );
            var token = tokens.EntityType;

            token.HasKey( t => t.Id );
            token.Property( t => t.Version ).IsConcurrencyToken();
            token.Action( "Activate" ).HasActionLink( c => LinkFor( c, "Activate", Minted, Reserved ), followsConventions );
            token.Action( "Deactivate" ).HasActionLink( c => LinkFor( c, "Deactivate", Activated ), followsConventions );
            token.Collection.Action( "Redeem" ).Parameter<string>( "code" ).OptionalParameter = false;
        }

        static bool ShouldAdvertiseAction( TokenState currentState, TokenState[] requiredStates ) => requiredStates.Contains( currentState );

        static Uri LinkFor( EntityInstanceContext context, string actionName, params TokenState[] requiredStates )
        {
            Contract.Requires( context != null );

            if ( !context.EdmObject.TryGetPropertyValue( "state", out var value ) )
            {
                return null;
            }

            if ( !ShouldAdvertiseAction( (TokenState) value, requiredStates ) )
            {
                return null;
            }

            var model = context.EdmModel;
            var qualifiedName = $"{model.EntityContainer.Namespace}.{actionName}";
            var operation = model.FindDeclaredBoundOperations( qualifiedName, context.EntityType ).Single();

            return context.GenerateActionLink( operation );
        }
    }
}