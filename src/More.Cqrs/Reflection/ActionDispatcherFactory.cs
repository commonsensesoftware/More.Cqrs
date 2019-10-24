﻿// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Reflection
{
    using System;
    using System.Collections.Concurrent;

    static class ActionDispatcherFactory<TArg>
    {
        static readonly ConcurrentDictionary<Type, object> builders = new ConcurrentDictionary<Type, object>();

        internal static IActionDispatcher<TArg> NewDispatcher<TInstance>( TInstance instance ) where TInstance : notnull
        {
            var builder = (IActionBuilder<TArg>) builders.GetOrAdd( instance.GetType(), CreateBuilder );
            return builder.Build( instance );
        }

        static object CreateBuilder( Type instanceType )
        {
            var builderType = typeof( ActionBuilder<,> ).MakeGenericType( instanceType, typeof( TArg ) );
            return Activator.CreateInstance( builderType, true )!;
        }
    }
}