namespace Contoso.Services.Composition.Conventions
{
    using More.ComponentModel;
    using System;

    sealed class RuntimeComponentConvention : IRule<ConventionContext>
    {
        public void Evaluate( ConventionContext context ) =>
            context.Conventions
                   .ForTypesMatching( TypeExtensions.IsRuntimeComponent )
                   .ExportInterfaces()
                   .Shared();
    }
}