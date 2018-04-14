// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    sealed class ActionDispatcher<TInstance, TArg> : IActionDispatcher<TArg>
    {
        readonly TInstance instance;
        readonly IDictionary<Type, Lazy<Action<TInstance, TArg>>> actionMap;

        internal ActionDispatcher( TInstance instance, IDictionary<Type, Lazy<Action<TInstance, TArg>>> actionMap )
        {
            Contract.Requires( instance != null );
            Contract.Requires( actionMap != null );

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