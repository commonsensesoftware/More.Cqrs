namespace Contoso.Services.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Composition.Convention;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    sealed class ConventionContext
    {
        internal ConventionContext( ConventionBuilder conventions, IEnumerable<Assembly> assemblies )
        {
            Contract.Requires( conventions != null );
            Contract.Requires( assemblies != null );

            Conventions = conventions;
            Types = new ComposableTypes( assemblies );
        }

        internal ConventionBuilder Conventions { get; }

        internal ComposableTypes Types { get; }

        internal ICollection<Type> ClosedGenericTypes { get; } = new HashSet<Type>();
    }
}