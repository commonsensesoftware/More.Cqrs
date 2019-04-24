// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using More.Domain.Sagas;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    static partial class TypeExtensions
    {
        internal static object DefaultValue( this Type type ) =>
            type.GetTypeInfo().IsValueType ? Activator.CreateInstance( type ) : null;

        internal static bool IsNullable( this Type type)
        {
            Contract.Requires( type != null );
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsValueType && typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition().Equals( typeof( Nullable<> ) );
        }

        internal static bool IsSaga( this Type type ) => type.EnumerateSagas().Any();

        internal static Type GetSagaDataType( this Type type )
        {
            Contract.Requires( type != null );
            Contract.Ensures( Contract.Result<Type>() != null );

            var sagaType = default( TypeInfo );

            try
            {
                sagaType = type.EnumerateSagas().SingleOrDefault();
            }
            catch ( InvalidOperationException )
            {
                throw new SagaConfigurationException( SR.MultipleSagaImplementations.FormatDefault( type.Name ) );
            }

            return sagaType.GenericTypeArguments[0];
        }

        static IEnumerable<TypeInfo> EnumerateSagas( this Type type )
        {
            Contract.Requires( type != null );
            Contract.Ensures( Contract.Result<IEnumerable<TypeInfo>>() != null );

            var typeInfo = type.GetTypeInfo();

            if ( typeInfo.IsAbstract || typeInfo.IsGenericType )
            {
                yield break;
            }

            var sagaType = typeof( ISaga<> ).GetTypeInfo();
            var interfaces = from @interface in typeInfo.ImplementedInterfaces.Select( i => i.GetTypeInfo() )
                             where @interface.IsInterface &&
                                   @interface.IsGenericType &&
                                   sagaType.IsAssignableFrom( @interface.GetGenericTypeDefinition().GetTypeInfo() )
                             select @interface;

            foreach ( var @interface in interfaces )
            {
                yield return @interface;
            }
        }
    }
}