namespace Contoso.Services.Composition
{
    using System;
    using System.Composition.Convention;
    using System.Reflection;

    sealed class ImportParameter
    {
        internal ImportParameter( ParameterInfo parameter, ImportConventionBuilder conventionBuilder )
        {
            Parameter = parameter;
            ConventionBuilder = conventionBuilder;
        }

        internal ParameterInfo Parameter { get; }

        internal ImportConventionBuilder ConventionBuilder { get; }
    }
}