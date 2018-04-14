namespace Contoso.Services.Composition
{
    using More.ComponentModel;
    using System;
    using System.Collections.Generic;
    using System.Composition.Convention;
    using System.Linq;
    using System.Reflection;

    sealed class DecoratedParameterRule : IRule<ImportParameter>
    {
        readonly HashSet<TypeInfo> decoratedTypes = new HashSet<TypeInfo>();
        readonly List<Tuple<string, object>> metadata = new List<Tuple<string, object>>();

        internal DecoratedParameterRule( params Type[] decoratedTypes )
        {
            this.decoratedTypes.AddRange( decoratedTypes.Select( t => t.GetTypeInfo() ) );
        }

        internal DecoratedParameterRule( IEnumerable<Type> decoratedTypes )
        {
            this.decoratedTypes.AddRange( decoratedTypes.Select( t => t.GetTypeInfo() ) );
        }

        internal string ContractName { get; set; } = "Decorated";

        internal void AddMetadata( string name, object value ) => metadata.Add( Tuple.Create( name, value ) );

        internal void Evaluate( ParameterInfo parameter, ImportConventionBuilder conventionBuilder ) =>
            Evaluate( new ImportParameter( parameter, conventionBuilder ) );

        public void Evaluate( ImportParameter item )
        {
            var type = item.Parameter.ParameterType.GetTypeInfo();

            if ( type.IsGenericType )
            {
                if ( type.IsGenericTypeDefinition )
                {
                    var matches = from decoratedType in decoratedTypes
                                  where decoratedType.IsGenericTypeDefinition &&
                                        decoratedType.IsAssignableFrom( type )
                                  select decoratedType;

                    if ( !matches.Any() )
                    {
                        return;
                    }
                }
                else
                {
                    var typeDef = type.GetGenericTypeDefinition().GetTypeInfo();
                    var matches = from decoratedType in decoratedTypes
                                  where decoratedType.IsAssignableFrom( type ) ||
                                        decoratedType.IsAssignableFrom( typeDef )
                                  select decoratedType;

                    if ( !matches.Any() )
                    {
                        return;
                    }
                }
            }
            else
            {
                if ( !decoratedTypes.Any( t => t.IsAssignableFrom( type ) ) )
                {
                    return;
                }
            }

            item.ConventionBuilder.AsContractName( ContractName );

            foreach ( var entry in metadata )
            {
                item.ConventionBuilder.AddMetadataConstraint( entry.Item1, entry.Item2 );
            }
        }
    }
}