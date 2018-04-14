using Contoso.Services;
using Microsoft.Owin;

[assembly: OwinStartup( typeof( Startup ) )]

namespace Contoso.Services
{
    using Configuration;
    using Microsoft.Owin.BuilderProperties;
    using More.Domain.Messaging;
    using Owin;
    using System;
    using System.Web.Http;
    using static Configuration.HttpConfigurationExtensions;
    using static System.IO.Path;

    public class Startup
    {
        MessageBus bus;

        public virtual void Configuration( IAppBuilder app )
        {
            var properties = new AppProperties( app.Properties );
            var configuration = new HttpConfiguration();
            var httpServer = new HttpServer( configuration );
            var assembliesResolver = configuration.Services.GetAssembliesResolver();
            var appSettingsPath = Combine( GetDirectoryName( AppDomain.CurrentDomain.SetupInformation.ConfigurationFile ), "appsettings.config" );
            var dependencyResolver = new CompositionDependencyResolver( assembliesResolver, appSettingsPath, ApplyWebApiConventions );
            var observers = new IObserver<IMessageDescriptor>[0];

            if ( app.Properties.TryGetValue( "bus.Observers", out object value ) )
            {
                observers = (IObserver<IMessageDescriptor>[]) value;
            }

            configuration.MessageHandlers.Add( new LogicalHttpRequestMessage() );
            configuration.Filters.Add( new SimpleAuthentiationFilter() );
            configuration.DependencyResolver = dependencyResolver;
            configuration.AddApiVersioning();
            configuration.ConfigureOData( httpServer );
            app.UseWebApi( httpServer );
            bus = dependencyResolver.GetRequiredService<MessageBus>();
            properties.OnAppDisposing.Register( ShutDownBus );
            bus.Start( observers );
        }

        void ShutDownBus()
        {
            bus.Stop().GetAwaiter().GetResult();
            bus.Dispose();
        }
    }
}