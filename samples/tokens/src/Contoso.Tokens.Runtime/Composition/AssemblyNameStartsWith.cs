namespace Contoso.Services.Composition
{
    using More.ComponentModel;
    using System;
    using System.Reflection;

    sealed class AssemblyNameStartsWith : SpecificationBase<AssemblyName>
    {
        readonly string assemblyName;

        internal AssemblyNameStartsWith( string assemblyName ) { this.assemblyName = assemblyName; }

        public override bool IsSatisfiedBy( AssemblyName item ) => item.Name.StartsWith( assemblyName );
    }
}