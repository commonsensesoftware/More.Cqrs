namespace Contoso.Services
{
    using System;
    using System.Collections.Generic;
    using System.Composition;
    using System.Web.Http.Dependencies;

    sealed class CompositionDependencyScope : IDependencyScope
    {
        readonly CompositionContext context;
        bool disposed;

        public CompositionDependencyScope( CompositionContext context ) => this.context = context;

        ~CompositionDependencyScope() => Dispose( false );

        private void Dispose( bool disposing )
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;

            if ( context is IDisposable disposable )
            {
                disposable.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        public object GetService( Type serviceType ) => GetService( serviceType, key: null );

        internal object GetService( Type serviceType, string key ) =>
            context.TryGetExport( serviceType, key, out var service ) ? service : null;

        public IEnumerable<object> GetServices( Type serviceType ) => context.GetExports( serviceType );
    }
}