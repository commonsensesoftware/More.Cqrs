namespace Contoso.Services
{
    using System;
    using static System.AttributeTargets;

    [AttributeUsage( Class, AllowMultiple = false, Inherited = false )]
    public sealed class RuntimeComponentAttribute : Attribute
    {
    }
}