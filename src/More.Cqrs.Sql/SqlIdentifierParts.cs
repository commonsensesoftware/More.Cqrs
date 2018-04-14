// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;

    /// <summary>
    /// Represents the possible database identifier parts.
    /// </summary>
    [Flags]
    public enum SqlIdentifierParts
    {
        /// <summary>
        /// Indicates schema name part.
        /// </summary>
        SchemaName = 1,

        /// <summary>
        /// Indicates object name part, such as a table.
        /// </summary>
        ObjectName = 2,

        /// <summary>
        ///  Indicates all parts.
        /// </summary>
        All = SchemaName | ObjectName,
    }
}