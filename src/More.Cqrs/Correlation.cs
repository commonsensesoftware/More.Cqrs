// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System.Threading;
    using static System.Guid;
    using static System.String;

    /// <summary>
    /// Represents a correlation identifier.
    /// </summary>
    /// <remarks>This object can be used to current and/or default correlation identifier
    /// that should be applied to new messages.</remarks>
    public static class Correlation
    {
        static readonly AsyncLocal<string> currentId = new AsyncLocal<string>();

        /// <summary>
        /// Gets or sets the current correlation identifier.
        /// </summary>
        /// <value>The current correlation identifier.</value>
        /// <remarks>
        /// <p>While the correlation identifier can be any string, it is recommended
        /// that a globally unique identifier (GUID) is used. If any explicit correlation
        /// identifier is not set, one will be generated as needed from a new GUID.</p>
        /// <p>The current correlation identifier is usually automatically updated for
        /// each incoming message. Explicitly setting the current correlation identifier
        /// should be avoided and is generally unnecessary, but is allowed for
        /// advanced or custom messages scenarios.</p>
        /// </remarks>
        public static string CurrentId
        {
            get
            {
                var value = currentId.Value;

                if ( IsNullOrEmpty( value ) )
                {
                    currentId.Value = value = NewGuid().ToString();
                }

                return value;
            }
            set
            {
                currentId.Value = value;
            }
        }
    }
}