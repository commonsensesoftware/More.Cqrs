namespace Contoso.Services.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    sealed class ComposableTypes
    {
        static readonly Type ExceptionType = typeof( Exception );
        static readonly Type AttributeType = typeof( Attribute );
        readonly Dictionary<Assembly, ExportedAssemblyTypes> exportedTypes = new Dictionary<Assembly, ExportedAssemblyTypes>();
        readonly IEnumerable<Assembly> assemblies;

        internal ComposableTypes( IEnumerable<Assembly> assemblies ) => this.assemblies = assemblies;

        internal IEnumerable<Type> ConcreteClasses()
        {
            foreach ( var assembly in assemblies )
            {
                if ( exportedTypes.TryGetValue( assembly, out var types ) )
                {
                    foreach ( var type in types.ConcreteClasses )
                    {
                        yield return type;
                    }
                }
                else
                {
                    types = new ExportedAssemblyTypes();

                    foreach ( var type in assembly.GetTypes() )
                    {
                        if ( NotComposable( type ) )
                        {
                            continue;
                        }

                        if ( type.IsClass )
                        {
                            if ( type.IsAbstract )
                            {
                                types.AbstractClasses.Add( type );
                            }
                            else
                            {
                                types.ConcreteClasses.Add( type );
                                yield return type;
                            }
                        }
                        else if ( type.IsInterface )
                        {
                            types.Interfaces.Add( type );
                        }
                    }

                    exportedTypes.Add( assembly, types );
                }

            }
        }

        internal IEnumerable<Type> AbstractClasses()
        {
            foreach ( var assembly in assemblies )
            {
                if ( exportedTypes.TryGetValue( assembly, out var types ) )
                {
                    foreach ( var type in types.AbstractClasses )
                    {
                        yield return type;
                    }
                }
                else
                {
                    types = new ExportedAssemblyTypes();

                    foreach ( var type in assembly.GetTypes() )
                    {
                        if ( NotComposable( type ) )
                        {
                            continue;
                        }

                        if ( type.IsClass )
                        {
                            if ( type.IsAbstract )
                            {
                                types.AbstractClasses.Add( type );
                                yield return type;
                            }
                            else
                            {
                                types.ConcreteClasses.Add( type );
                            }
                        }
                        else if ( type.IsInterface )
                        {
                            types.Interfaces.Add( type );
                        }
                    }

                    exportedTypes.Add( assembly, types );
                }

            }
        }

        internal IEnumerable<Type> Interfaces()
        {
            foreach ( var assembly in assemblies )
            {
                if ( exportedTypes.TryGetValue( assembly, out var types ) )
                {
                    foreach ( var type in types.Interfaces )
                    {
                        yield return type;
                    }
                }
                else
                {
                    types = new ExportedAssemblyTypes();

                    foreach ( var type in assembly.GetTypes() )
                    {
                        if ( NotComposable( type ) )
                        {
                            continue;
                        }

                        if ( type.IsClass )
                        {
                            if ( type.IsAbstract )
                            {
                                types.AbstractClasses.Add( type );
                            }
                            else
                            {
                                types.ConcreteClasses.Add( type );
                            }
                        }
                        else if ( type.IsInterface )
                        {
                            types.Interfaces.Add( type );
                            yield return type;

                        }
                    }

                    exportedTypes.Add( assembly, types );
                }

            }
        }

        static bool NotComposable( Type type ) =>
            !type.IsPublic || ExceptionType.IsAssignableFrom( type ) || AttributeType.IsAssignableFrom( type );

        sealed class ExportedAssemblyTypes
        {
            internal IList<Type> ConcreteClasses { get; } = new List<Type>();

            internal IList<Type> AbstractClasses { get; } = new List<Type>();

            internal IList<Type> Interfaces { get; } = new List<Type>();
        }
    }
}