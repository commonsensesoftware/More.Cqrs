// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Reflection
{
    using System;

    interface IActionDispatcher<in TArg>
    {
        void Invoke( TArg arg );
    }
}