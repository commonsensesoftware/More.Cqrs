namespace Contoso.Services.Composition
{
    using System;
    using System.Reflection;
    using static System.Attribute;

    static class TypeExtensions
    {
        internal static bool IsRuntimeComponent( this Type type ) => IsDefined( type, typeof( RuntimeComponentAttribute ), inherit: false );
    }
}