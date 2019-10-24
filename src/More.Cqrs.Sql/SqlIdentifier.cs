// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;
    using System.Text;
    using static System.Char;
    using static System.StringComparer;

    /// <summary>
    /// Represents a database object identifier.
    /// <seealso href="!https://docs.microsoft.com/en-us/sql/relational-databases/databases/database-identifiers"/>
    /// </summary>
    public struct SqlIdentifier : IEquatable<SqlIdentifier>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlIdentifier"/> structure.
        /// </summary>
        /// <param name="objectName">The database object name.</param>
        /// <remarks>This constructor always uses the "dbo" schema.</remarks>
        public SqlIdentifier( string objectName ) : this( "dbo", objectName ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlIdentifier"/> structure.
        /// </summary>
        /// <param name="schemaName">The database schema name.</param>
        /// <param name="objectName">The database object name.</param>
        public SqlIdentifier( string schemaName, string objectName )
        {
            SchemaName = schemaName;
            ObjectName = objectName;
        }

        /// <summary>
        /// Gets the schema name part.
        /// </summary>
        /// <value>The schema component of the name.</value>
        public string SchemaName { get; }

        /// <summary>
        /// Gets the object name part.
        /// </summary>
        /// <value>The object component of the name.</value>
        public string ObjectName { get; }

        /// <summary>
        /// Returns the identifier in its delimited form.
        /// </summary>
        /// <param name="parts">The identifier parts to delimit. The default value is all parts.</param>
        /// <returns>The identifier name in its delimited form.</returns>
        public string Delimit( SqlIdentifierParts parts = SqlIdentifierParts.All )
        {
            var text = new StringBuilder();

            if ( parts.HasFlag( SqlIdentifierParts.SchemaName ) )
            {
                text.Append( '[' );
                text.Append( SchemaName );
                text.Append( ']' );
            }

            if ( parts.HasFlag( SqlIdentifierParts.ObjectName ) )
            {
                if ( text.Length > 0 )
                {
                    text.Append( '.' );
                }

                text.Append( '[' );
                text.Append( ObjectName );
                text.Append( ']' );
            }

            return text.ToString();
        }

        /// <summary>
        /// Returns the text representation of the object name.
        /// </summary>
        /// <returns>The database object name.</returns>
        public override string ToString() => Delimit();

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode() =>
            HashCode.Combine( OrdinalIgnoreCase.GetHashCode( SchemaName ), OrdinalIgnoreCase.GetHashCode( ObjectName ) );

        /// <summary>
        /// Determines whether the current instance equals the specified object.
        /// </summary>
        /// <param name="obj">The other object to compare against.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public override bool Equals( object? obj ) => obj is SqlIdentifier other && Equals( other );

        /// <summary>
        /// Determines whether the current instance equals the specified object.
        /// </summary>
        /// <param name="other">The other object to compare against.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public bool Equals( SqlIdentifier other )
        {
            return OrdinalIgnoreCase.Equals( SchemaName, other.SchemaName ) &&
                   OrdinalIgnoreCase.Equals( ObjectName, other.ObjectName );
        }

        /// <summary>
        /// Returns the object name in a form suitable for an identifier name.
        /// </summary>
        /// <param name="parts">The identifier parts to delimit. The default value is all parts.</param>
        /// <returns>The object name as a safe, identifier name.</returns>
        /// <remarks>This method can be used to return the object name in a safe form
        /// that can be used in identifiers. For example, if an index is to be created
        /// for a table, the format "IX_{name}" might be used. If the qualified object
        /// name is [dbo].[My Table], this method would return "dbo_My_Table" that can
        /// subsequently used to build the index identifier "IX_dbo_My_Table".</remarks>
        public string AsIdentifierName( SqlIdentifierParts parts = SqlIdentifierParts.All )
        {
            var text = new StringBuilder();

            if ( parts.HasFlag( SqlIdentifierParts.SchemaName ) )
            {
                for ( var i = 0; i < SchemaName.Length; i++ )
                {
                    var @char = SchemaName[i];
                    text.Append( IsLetterOrDigit( @char ) ? @char : '_' );
                }
            }

            if ( parts.HasFlag( SqlIdentifierParts.ObjectName ) )
            {
                if ( text.Length > 0 )
                {
                    text.Append( '_' );
                }

                for ( var i = 0; i < ObjectName.Length; i++ )
                {
                    var @char = ObjectName[i];
                    text.Append( IsLetterOrDigit( @char ) ? @char : '_' );
                }
            }

            return text.ToString();
        }

        /// <summary>
        /// Overrides the default equality operator.
        /// </summary>
        /// <param name="name">The object to compare.</param>
        /// <param name="otherName">The object to compare against.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public static bool operator ==( SqlIdentifier name, SqlIdentifier otherName ) => name.Equals( otherName );

        /// <summary>
        /// Overrides the default inequality operator.
        /// </summary>
        /// <param name="name">The object to compare.</param>
        /// <param name="otherName">The object to compare against.</param>
        /// <returns>True if the objects are not equal; otherwise, false.</returns>
        public static bool operator !=( SqlIdentifier name, SqlIdentifier otherName ) => !name.Equals( otherName );

        /// <summary>
        /// Implicitly converts the structure to its string representation.
        /// </summary>
        /// <param name="sqlName">The <see cref="SqlIdentifier">SQL object name</see> to convert.</param>
        public static implicit operator string( SqlIdentifier sqlName ) => sqlName.ToString();
    }
}