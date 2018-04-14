namespace Contoso.Services.Configuration
{
    using Contoso.Services.Tracing;
    using Microsoft.OData.Edm;
    using Microsoft.OData.Edm.Library;
    using Microsoft.Web.OData.Builder;
    using System;
    using System.Collections.Generic;
    using System.Composition.Convention;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Http;
    using System.Web.Http.Tracing;
    using System.Web.OData;
    using System.Web.OData.Batch;
    using System.Web.OData.Builder;
    using System.Web.OData.Extensions;

    static class HttpConfigurationExtensions
    {
        const string RoutePrefix = null;

        internal static void ConfigureOData( this HttpConfiguration configuration, HttpServer httpServer )
        {
            // BUG: cannot use alt keys and unqualified actions in 5.9.1; may be resolvable in 6.0, but that isn't not supported by API versioning - yet
            // REF: https://github.com/OData/WebApi/issues/636

            //configuration.EnableAlternateKeys( true );
            configuration.EnableCaseInsensitive( true );
            configuration.EnableUnqualifiedNameCall( true );
            configuration.EnableEnumPrefixFree( true );
            configuration.EnableMediaResources();

            var modelConfigurations = configuration.DependencyResolver.GetServices<IModelConfiguration>();
            var builder = new VersionedODataModelBuilder( configuration )
            {
                ModelBuilderFactory = () => new ODataConventionModelBuilder() { Namespace = nameof( Contoso ) }.EnableLowerCamelCase(),
                OnModelCreated = BuilderExtensions.ApplyAnnotations
            };

            builder.ModelConfigurations.AddRange( modelConfigurations );

            var models = builder.GetEdmModels();
            var batchHandler = new DefaultODataBatchHandler( httpServer );

            configuration.MapVersionedODataRoutes( "odata", RoutePrefix, models, batchHandler );
        }

        static void AfterModelCreated( ODataModelBuilder modelBuilder, IEdmModel model )
        {
            AddAlternateKeyForTokens( model );
        }

        static void AddAlternateKeyForTokens( IEdmModel edmModel )
        {
            var model = edmModel as EdmModel;

            if ( model == null )
            {
                return;
            }

            var tokens = model.EntityContainer.FindEntitySet( "Tokens" );

            if ( tokens == null )
            {
                return;
            }

            var token = tokens.EntityType();
            var property = MapClrPropertyToEdmProperty( model, token, ( Tokens.Token t ) => t.Code );
            var alternateKey = new Dictionary<string, IEdmProperty>() { [property.Name] = property };

            // REF: http://odata.github.io/WebApi/#04-17-Alternate-Key
            model.AddAlternateKeyAnnotation( token, alternateKey );
        }

        static IEdmProperty MapClrPropertyToEdmProperty<TEntity, TValue>( IEdmModel model, IEdmEntityType entity, Expression<Func<TEntity, TValue>> propertyExpression )
        {
            Contract.Requires( model != null );
            Contract.Requires( entity != null );
            Contract.Requires( propertyExpression != null );
            Contract.Ensures( Contract.Result<IEdmProperty>() != null );

            var propertyInfo = ( (MemberExpression) propertyExpression.Body ).Member;
            var properties = from property in entity.Properties()
                             let annotation = model.GetAnnotationValue<ClrPropertyInfoAnnotation>( property )
                             where annotation.ClrPropertyInfo == propertyInfo
                             select property;

            return properties.Single();
        }

        internal static void ApplyWebApiConventions( ConventionBuilder conventions )
        {
            Contract.Requires( conventions != null );

            conventions.ForTypesDerivedFrom<IModelConfiguration>().Export<IModelConfiguration>();
            conventions.ForType<TraceWriter>().Export<ITraceWriter>().Shared();
        }
    }
}