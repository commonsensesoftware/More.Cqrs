namespace Contoso.Services.Composition
{
    using More.ComponentModel;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web.Http.Dispatcher;

    sealed class ComposedAssemblies : IEnumerable<Assembly>
    {
        readonly ISpecification<AssemblyName> filter =
                 new AssemblyNameStartsWith( nameof( Contoso ) )
           .Or(  new AssemblyNameStartsWith( nameof( More ) ) )
           .And( new AssemblyNameEndsWith( "Tests" ).Not() );

        readonly HashSet<AssemblyName> filteredAssemblies = new HashSet<AssemblyName>();
        readonly IAssembliesResolver resolver;

        internal ComposedAssemblies( IAssembliesResolver resolver ) => this.resolver = resolver;

        public IEnumerator<Assembly> GetEnumerator()
        {
            foreach ( var assembly in resolver.GetAssemblies() )
            {
                var name = assembly.GetName();

                if ( filteredAssemblies.Contains( name ) )
                {
                    yield return assembly;
                }

                if ( filter.IsSatisfiedBy( name ) )
                {
                    filteredAssemblies.Add( name );
                    yield return assembly;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}