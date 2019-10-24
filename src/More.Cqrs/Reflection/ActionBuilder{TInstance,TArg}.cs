// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using static System.Diagnostics.Debug;
    using static System.Linq.Expressions.Expression;

#pragma warning disable CA1812
    sealed class ActionBuilder<TInstance, TArg> : IActionBuilder<TArg>
        where TInstance : notnull
        where TArg : notnull
#pragma warning restore CA1812
    {
        readonly Lazy<IDictionary<Type, Lazy<Action<TInstance, TArg>>>> methodMap;

        internal ActionBuilder() => methodMap = new Lazy<IDictionary<Type, Lazy<Action<TInstance, TArg>>>>( CreateMethodMap );

        static IDictionary<Type, Lazy<Action<TInstance, TArg>>> CreateMethodMap()
        {
            var validator = new ArgumentTypeValidator( typeof( TArg ) );
            var @void = typeof( void );
            var matches = from method in typeof( TInstance ).GetRuntimeMethods()
                          where method.ReturnType == @void
                          let args = method.GetParameters()
                          where args.Length == 1
                          let argType = args[0].ParameterType
                          where validator.IsValid( argType )
                          select new { Type = argType, Method = method };

            return matches.ToDictionary( m => m.Type, m => NewActionHolder( m.Type, m.Method ) );
        }

        static Lazy<Action<TInstance, TArg>> NewActionHolder( Type eventType, MethodInfo method ) =>
            new Lazy<Action<TInstance, TArg>>( () => MapAction( eventType, method ) );

        static Action<TInstance, TArg> MapAction( Type eventType, MethodInfo method )
        {
            var instance = Parameter( typeof( TInstance ), "instance" );
            var arg = Parameter( typeof( TArg ), "arg" );
            var body = Call( instance, method, Convert( arg, eventType ) );
            var lambda = Lambda<Action<TInstance, TArg>>( body, instance, arg );

            WriteLine( lambda );

            return lambda.Compile();
        }

        public IActionDispatcher<TArg> Build( object instance ) => new ActionDispatcher<TInstance, TArg>( (TInstance) instance, methodMap.Value );
    }
}