namespace Contoso.Services.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Web.Http.Dependencies;

    static class IDependencyResolverExtensions
    {
        internal static IEnumerable<TService> GetServices<TService>( this IDependencyResolver dependencyResolver )
        {
            foreach ( object service in dependencyResolver.GetServices( typeof( TService ) ) )
            {
                yield return (TService) service;
            }
        }
    }
}