// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;
    using System.Reflection;
    using System.Text;

    static partial class TypeExtensions
    {
        internal static string GetAssemblyQualifiedName( this Type type ) => type.GetTypeInfo().GetAssemblyQualifiedName();

        internal static string GetAssemblyQualifiedName( this TypeInfo typeInfo )
        {
            var name = new StringBuilder();

            name.Append( typeInfo.FullName );
            name.Append( ", " );
            name.Append( typeInfo.Assembly.GetName().Name );

            return name.ToString();
        }
    }
}