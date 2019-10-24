// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System.IO;
    using static System.Convert;
    using static System.Security.Cryptography.HashAlgorithmName;
    using static System.Security.Cryptography.IncrementalHash;
    using static System.Text.Encoding;

    /// <summary>
    /// Provides methods for computing checksums.
    /// </summary>
    public static partial class Checksum
    {
        /// <summary>
        /// Computes a checksum for the specified text and returns the result as a Base64 string.
        /// </summary>
        /// <param name="text">The text to compute a checksum for.</param>
        /// <returns>The checksum as a Base64 string.</returns>
        public static string AsBase64( string text ) => ToBase64String( AsBinary( UTF8.GetBytes( text ) ) );

        /// <summary>
        /// Computes a checksum for the specified source and returns the result as a Base64 string.
        /// </summary>
        /// <param name="source">The source content to compute a checksum for.</param>
        /// <returns>The checksum as a Base64 string.</returns>
        public static string AsBase64( byte[] source ) => ToBase64String( AsBinary( source ) );

        /// <summary>
        /// Computes a checksum for the specified stream and returns the result as a Base64 string.
        /// </summary>
        /// <param name="stream">The <see cref="Stream">stream</see> to compute a checksum for.</param>
        /// <returns>The checksum as a Base64 string.</returns>
        public static string AsBase64( Stream stream ) => ToBase64String( AsBinary( stream ) );

        /// <summary>
        /// Computes a checksum for the specified text and returns the result in binary form.
        /// </summary>
        /// <param name="text">The text to compute a checksum for.</param>
        /// <returns>The checksum in binary form.</returns>
        public static byte[] AsBinary( string text ) => AsBinary( UTF8.GetBytes( text ) );

        /// <summary>
        /// Creates and returns a checksum for the specified source as binary content.
        /// </summary>
        /// <param name="source">The source data to compute a checksum for.</param>
        /// <returns>The computed checksum in binary form.</returns>
        public static byte[] AsBinary( byte[] source )
        {
            using var sha1 = CreateHash( SHA1 );
            sha1.AppendData( source );
            return sha1.GetHashAndReset();
        }

        /// <summary>
        /// Creates and returns a checksum for the specified stream as binary content.
        /// </summary>
        /// <param name="stream">The <see cref="Stream">stream</see> to compute a checksum for.</param>
        /// <returns>The computed checksum in binary form.</returns>
        public static byte[] AsBinary( Stream stream ) => AsBinary( stream, 0x200 );

        /// <summary>
        /// Creates and returns a checksum for the specified stream as binary content.
        /// </summary>
        /// <param name="stream">The <see cref="Stream">stream</see> to compute a checksum for.</param>
        /// <param name="bufferSize">The buffer size to use while reading the <paramref name="stream"/>.</param>
        /// <returns>The computed checksum in binary form.</returns>
        public static byte[] AsBinary( Stream stream, int bufferSize )
        {
            var buffer = new byte[bufferSize];
            var count = stream.Read( buffer, 0, bufferSize );
            using var sha1 = CreateHash( SHA1 );

            sha1.AppendData( buffer, 0, count );

            while ( count == bufferSize )
            {
                count = stream.Read( buffer, 0, bufferSize );
                sha1.AppendData( buffer, 0, count );
            }

            return sha1.GetHashAndReset();
        }
    }
}