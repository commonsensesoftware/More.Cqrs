namespace Contoso.Services.Composition
{
    using More.ComponentModel;
    using System;
    using System.Reflection;

    sealed class AssemblyNameEndsWith : SpecificationBase<AssemblyName>
    {
        readonly string assemblyName;

        internal AssemblyNameEndsWith( string assemblyName ) { this.assemblyName = assemblyName; }

        public override bool IsSatisfiedBy( AssemblyName item ) => item.Name.EndsWith( assemblyName );
    }
}