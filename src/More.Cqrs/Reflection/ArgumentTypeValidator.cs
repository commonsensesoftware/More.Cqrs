// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Reflection
{
    using System;
    using System.Reflection;

    sealed class ArgumentTypeValidator
    {
        readonly Type filterArgType;
        readonly TypeInfo filterArgTypeInfo;

        internal ArgumentTypeValidator( Type filterArgType )
        {
            this.filterArgType = filterArgType;
            filterArgTypeInfo = filterArgType.GetTypeInfo();
        }

        internal bool IsValid( Type targetArgType ) =>
            targetArgType != filterArgType &&
            filterArgTypeInfo.IsAssignableFrom( targetArgType.GetTypeInfo() );
    }
}