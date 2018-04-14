namespace Contoso.Services.Composition
{
    using More.ComponentModel;
    using System;
    using System.Collections.Generic;
    using System.Composition;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    sealed class ConstructorSelectionRule : IRule<IEnumerable<ConstructorInfo>, ConstructorInfo>
    {
        static readonly Lazy<ConstructorSelectionRule> parameterless = new Lazy<ConstructorSelectionRule>( CreateParameterlessRule );
        readonly Func<IEnumerable<ConstructorInfo>, ConstructorInfo> selector;
        readonly Type[] parameterTypes;

        ConstructorSelectionRule( Func<IEnumerable<ConstructorInfo>, ConstructorInfo> selector ) => this.selector = selector;

        internal ConstructorSelectionRule() => selector = DefaultSelector;

        internal ConstructorSelectionRule( IEnumerable<Type> parameterTypes )
        {
            this.parameterTypes = parameterTypes.ToArray();
            selector = MatchingTypesSelector;
        }

        internal ConstructorSelectionRule( params Type[] parameterTypes )
        {
            this.parameterTypes = parameterTypes;
            selector = MatchingTypesSelector;
        }

        internal static ConstructorSelectionRule Parameterless => parameterless.Value;

        static ConstructorSelectionRule CreateParameterlessRule() =>
            new ConstructorSelectionRule( ctors => ctors.Single( ctor => ctor.GetParameters().Length == 0 ) );

        static ConstructorInfo DefaultSelector( IEnumerable<ConstructorInfo> constructors )
        {
            var constructor = default( ConstructorInfo );
            var importingConstructor = typeof( ImportingConstructorAttribute );

            using ( var iterator = constructors.OrderByDescending( c => c.GetParameters().Length ).GetEnumerator() )
            {
                if ( !iterator.MoveNext() )
                {
                    return constructor;
                }

                constructor = iterator.Current;

                if ( constructor.CustomAttributes.Any( c => c.AttributeType == importingConstructor ) )
                {
                    return constructor;
                }

                while ( iterator.MoveNext() )
                {
                    var current = iterator.Current;

                    if ( current.CustomAttributes.Any( c => c.AttributeType == importingConstructor ) )
                    {
                        constructor = current;
                        break;
                    }
                }
            }

            return constructor;
        }

        ConstructorInfo MatchingTypesSelector( IEnumerable<ConstructorInfo> constructors )
        {
            Contract.Requires( constructors != null );
            Contract.Ensures( Contract.Result<ConstructorInfo>() != null );

            var matches = from constructor in constructors
                          let paramTypes = constructor.GetParameters().Select( p => p.ParameterType )
                          where paramTypes.SequenceEqual( parameterTypes, TypeComparer.Instance )
                          select constructor;

            return matches.SingleOrDefault();
        }

        public ConstructorInfo Evaluate( IEnumerable<ConstructorInfo> item ) => selector( item );

        sealed class TypeComparer : IEqualityComparer<Type>
        {
            private TypeComparer() { }

            internal static IEqualityComparer<Type> Instance { get; } = new TypeComparer();

            static Type UnwrapType( Type type ) => type == null || !type.GetTypeInfo().IsGenericType ? type : type.GetGenericTypeDefinition();

            public bool Equals( Type x, Type y )
            {
                var a = UnwrapType( x );
                var b = UnwrapType( y );

                if ( a == null )
                {
                    return b == null;
                }

                return a.Equals( b );
            }

            public int GetHashCode( Type obj ) => UnwrapType( obj )?.GetHashCode() ?? 0;
        }
    }
}