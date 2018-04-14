// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Reflection
{
    using System;

    interface IActionBuilder<TArg>
    {
        IActionDispatcher<TArg> Build( object instance );
    }
}