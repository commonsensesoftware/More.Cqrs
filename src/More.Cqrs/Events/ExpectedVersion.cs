// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Represents the expected version of an event.
    /// </summary>
    [DebuggerDisplay( "{value}" )]
    public struct ExpectedVersion : IEquatable<ExpectedVersion>, IEquatable<int>
    {
        readonly int value;

        private ExpectedVersion( int value ) => this.value = value;

        /// <summary>
        /// Gets the initial version of an event.
        /// </summary>
        /// <value>An <see cref="ExpectedVersion">expected version</see> that represents the initial version.</value>
        public static ExpectedVersion Initial { get; } = new ExpectedVersion( -1 );

        /// <summary>
        /// Gets any version of an event.
        /// </summary>
        /// <value>An <see cref="ExpectedVersion">expected version</see> that represents any version.</value>
        public static ExpectedVersion Any { get; } = new ExpectedVersion( -2 );

        /// <summary>
        /// Implicitly converts the event version to an integer.
        /// </summary>
        /// <param name="version">The instance to convert.</param>
#pragma warning disable CA2225 // Operator overloads have named alternates
        public static implicit operator int( ExpectedVersion version ) => version.value;
#pragma warning restore CA2225 // Operator overloads have named alternates

        /// <summary>
        /// Compares the current instance to an integer for equality.
        /// </summary>
        /// <param name="version">The instance to compare.</param>
        /// <param name="value">The integer to compare against.</param>
        /// <returns>True if the instance is equivalent to the integer; otherwise, false.</returns>
        public static bool operator ==( ExpectedVersion version, int value ) => version.value == value;

        /// <summary>
        /// Compares an integer to the current instance for equality.
        /// </summary>
        /// <param name="value">The integer to compare.</param>
        /// <param name="version">The instance to compare against.</param>
        /// <returns>True if the integer is equivalent to the instance; otherwise, false.</returns>
        public static bool operator ==( int value, ExpectedVersion version ) => version.value == value;

        /// <summary>
        /// Compares the current instance to an integer for inequality.
        /// </summary>
        /// <param name="version">The instance to compare.</param>
        /// <param name="value">The integer to compare against.</param>
        /// <returns>True if the instance is not equivalent to the integer; otherwise, false.</returns>
        public static bool operator !=( ExpectedVersion version, int value ) => version.value != value;

        /// <summary>
        /// Compares an integer to the current instance for inequality.
        /// </summary>
        /// <param name="value">The integer to compare.</param>
        /// <param name="version">The instance to compare against.</param>
        /// <returns>True if the integer is not equivalent to the instance; otherwise, false.</returns>
        public static bool operator !=( int value, ExpectedVersion version ) => version.value != value;

        /// <summary>
        /// Determines whether the current instance equals the specified object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if the current instance equals the specified object; otherwise, false.</returns>
        public override bool Equals( object? obj ) =>
            ( obj is ExpectedVersion eventVersion && Equals( eventVersion ) ) ||
            ( obj is int value && Equals( value ) );

        /// <summary>
        /// Determines whether the current instance equals the other instance.
        /// </summary>
        /// <param name="other">The other instance to compare.</param>
        /// <returns>True if the current instance equals the other instance; otherwise, false.</returns>
        public bool Equals( ExpectedVersion other ) => other.value == value;

        /// <summary>
        /// Gets a hash code for the current instance.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode() => value.GetHashCode();

        /// <summary>
        /// Determines whether the current instance equals the other value.
        /// </summary>
        /// <param name="other">The value to compare.</param>
        /// <returns>True if the current instance equals the other instance; otherwise, false.</returns>
        public bool Equals( int other ) => other == value;
    }
}