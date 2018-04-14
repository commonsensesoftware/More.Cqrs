// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;
    using static System.Runtime.ExceptionServices.ExceptionDispatchInfo;

    static partial class ExceptionExtensions
    {
        internal static void Rethrow( this Exception exception ) => Capture( exception ).Throw();
    }
}