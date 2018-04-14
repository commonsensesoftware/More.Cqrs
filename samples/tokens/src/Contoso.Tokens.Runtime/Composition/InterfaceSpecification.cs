namespace Contoso.Services.Composition
{
    using More.ComponentModel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    sealed class InterfaceSpecification : SpecificationBase<Type>
    {
        readonly bool exactMatch;
        readonly TypeInfo interfaceType;

        internal InterfaceSpecification( Type interfaceType, bool exactMatch = false )
        {
            this.interfaceType = interfaceType.GetTypeInfo();
            this.exactMatch = exactMatch;
        }

        public override bool IsSatisfiedBy( Type item )
        {
            if ( item == null || !interfaceType.IsInterface )
            {
                return false;
            }

            var target = item.GetTypeInfo();
            var interfaces = default( IEnumerable<TypeInfo> );

            if ( !target.IsPublic )
            {
                return false;
            }

            if ( target.IsInterface )
            {
                interfaces = new[] { target.GetTypeInfo() };
            }
            else if ( target.IsAbstract )
            {
                return false;
            }
            else
            {
                interfaces = target.ImplementedInterfaces.Select( t => t.GetTypeInfo() );
            }

            var match = interfaces.Any( i => IsMatch( interfaceType, i, exactMatch ) );

            return match;
        }

        static bool IsMatch( TypeInfo source, TypeInfo target, bool exactMatch )
        {
            var typeDef = target.IsGenericType ? target.GetGenericTypeDefinition().GetTypeInfo() : target;

            if ( exactMatch )
            {
                return source.Equals( typeDef );
            }
            else
            {
                if ( source.IsAssignableFrom( typeDef ) )
                {
                    return true;
                }

                foreach ( var @interface in typeDef.ImplementedInterfaces )
                {
                    if ( @interface.GetTypeInfo().IsGenericType && source.IsAssignableFrom( @interface.GetGenericTypeDefinition().GetTypeInfo() ) )
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}