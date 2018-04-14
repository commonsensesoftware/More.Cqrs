namespace Contoso.Services
{
    using Composition;
    using Contoso.Domain.Tokens;
    using Contoso.Services.Composition.Conventions;
    using Microsoft.Extensions.Configuration;
    using More.ComponentModel;
    using More.Composition;
    using More.Domain;
    using More.Domain.Events;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Composition;
    using System.Composition.Convention;
    using System.Composition.Hosting;
    using System.Composition.Hosting.Core;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http.Dependencies;
    using System.Web.Http.Dispatcher;
    using static DefaultConfigurationSettings;

    public sealed class CompositionDependencyResolver : IDependencyResolver, IServiceProvider
    {
        static readonly Type IEnumerableOfT = typeof( IEnumerable<> ).GetTypeInfo();
        static readonly MethodInfo GetTypedServicesOfT = typeof( CompositionDependencyResolver ).GetTypeInfo().GetRuntimeMethods().Single( m => m.Name == nameof( GetTypedServices ) );
        readonly ConcurrentDictionary<Type, Func<string, object>> getServices = new ConcurrentDictionary<Type, Func<string, object>>();
        readonly CompositionHost host;
        readonly Lazy<IConfigurationRoot> settings;
        bool disposed;

        public CompositionDependencyResolver( IAssembliesResolver assembliesResolver, string appSettingsPath = null, Action<ConventionBuilder> webApiConventions = null )
        {
            Contract.Requires( assembliesResolver != null );

            var configuration = new ContainerConfiguration();
            var conventions = new ConventionBuilder();
            var assemblies = new ComposedAssemblies( assembliesResolver );
            var context = new ConventionContext( conventions, assemblies );
            var rules = new IRule<ConventionContext>[]
            {
                new RequiredComponentConvention(),
                new RuntimeComponentConvention(),
                new MessageBusConvention(),
                new AggregateConvention(),
                new CommandAndEventConvention(),
                new ReadModelRepositoryConvention(),
                new ControllerConvention()
            };

            settings = new Lazy<IConfigurationRoot>( () => CreateSettingsConfiguration( appSettingsPath ) );
            rules.ForEach( rule => rule.Evaluate( context ) );
            configuration.WithDefaultConventions( conventions );
            configuration.WithAssemblies( assemblies );
            configuration.WithProvider( new SelfExportDescriptorProvider( this ) );
            configuration.WithProvider( new ConfigurationExportProvider( ( key, type ) => settings.Value.GetValue( type, key ) ) );
            configuration.WithProvider( new SqlConfigurationExportDescriptorProvider( ConnectionStringKey, SubscriptionKey ) );
            configuration.WithProvider( new SagaMetadataExportDescriptorProvider( context ) );
            configuration.WithProvider( new HttpRequestExportDescriptorProvider() );
            configuration.WithParts( context.ClosedGenericTypes );
            webApiConventions?.Invoke( conventions );
            host = configuration.CreateContainer();
        }

        public IDependencyScope BeginScope()
        {
            const string contractName = default( string );

            var boundaryNames = new Dictionary<string, object> { { "SharingBoundaryNames", new[] { "PerRequest" } } };
            var contract = new CompositionContract( typeof( ExportFactory<CompositionContext> ), contractName, boundaryNames );
            var perRequestFactory = (ExportFactory<CompositionContext>) host.GetExport( contract );

            return new CompositionDependencyScope( perRequestFactory.CreateExport().Value );
        }

        public void Dispose()
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;
            host.Dispose();
        }

        public object GetService( Type serviceType ) => GetService( serviceType, key: null );

        public object GetService( Type serviceType, string key )
        {
            var typeInfo = serviceType?.GetTypeInfo() ?? throw new ArgumentNullException( nameof( serviceType ) );

            if ( typeInfo.IsArray )
            {
                serviceType = typeInfo.GetElementType();

                var services = host.GetExports( serviceType, key ).ToArray();
                var array = Array.CreateInstance( serviceType, services.Length );

                services.CopyTo( array, 0 );

                return array;
            }

            if ( typeInfo.IsGenericType && IEnumerableOfT.IsAssignableFrom( typeInfo.GetGenericTypeDefinition() ) )
            {
                serviceType = typeInfo.GenericTypeArguments[0];
                return getServices.GetOrAdd( serviceType, t => (Func<string, object>) GetTypedServicesOfT.MakeGenericMethod( t ).CreateDelegate( typeof( Func<string, object> ), this ) )( key );
            }

            return host.TryGetExport( serviceType, key, out var service ) ? service : null;
        }

        object GetTypedServices<TService>( string key ) => host.GetExports<TService>( key );

        public IEnumerable<object> GetServices( Type serviceType ) => host.GetExports( serviceType );

        static IConfigurationRoot CreateSettingsConfiguration( string appSettingsPath )
        {
            var builder = new ConfigurationBuilder();

            builder.AddInMemoryCollection( new DefaultConfigurationSettings() );

            if ( !string.IsNullOrEmpty( appSettingsPath ) )
            {
                builder.AddXmlFile( appSettingsPath, optional: true );
            }

            return builder.Build();
        }
    }
}