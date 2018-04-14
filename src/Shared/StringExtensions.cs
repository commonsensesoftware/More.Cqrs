// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using static System.Globalization.CultureInfo;
    using static System.String;

    static class StringExtensions
    {
        [Pure]
        [DebuggerStepThrough]
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Reserved; not necessarily used in all code paths." )]
        internal static string FormatInvariant( this string format, params object[] args ) => Format( InvariantCulture, format, args );

        [Pure]
        [DebuggerStepThrough]
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Reserved; not necessarily used in all code paths." )]
        public static string FormatDefault( this string format, params object[] args ) => Format( CurrentCulture, format, args );
    }
}