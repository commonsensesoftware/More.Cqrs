// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;

    sealed class ServiceProvider : IServiceProvider
    {
        ServiceProvider() { }

        internal static IServiceProvider Default { get; } = new ServiceProvider();

        public object? GetService( Type serviceType ) => null;
    }
}