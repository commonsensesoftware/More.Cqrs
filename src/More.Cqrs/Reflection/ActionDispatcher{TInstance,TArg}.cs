// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Reflection
{
    using System;
    using System.Collections.Generic;

    sealed class ActionDispatcher<TInstance, TArg> : IActionDispatcher<TArg> where TArg : notnull
    {
        readonly TInstance instance;
        readonly IDictionary<Type, Lazy<Action<TInstance, TArg>>> actionMap;

        internal ActionDispatcher( TInstance instance, IDictionary<Type, Lazy<Action<TInstance, TArg>>> actionMap )
        {
            this.instance = instance;
            this.actionMap = actionMap;
        }

        public void Invoke( TArg arg )
        {
            var key = arg.GetType();

            if ( actionMap.TryGetValue( key, out var action ) )
            {
                action.Value( instance, arg );
                return;
            }

            throw new MissingMethodException( SR.SingleParameterMethodNotFound.FormatDefault( key.Name ) );
        }
    }
}