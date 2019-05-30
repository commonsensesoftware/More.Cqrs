// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;
    using System.IO;

    static class StreamExtensions
    {
        internal static byte[] ToBytes( this Stream stream )
        {
            using ( stream )
            {
                if ( stream is MemoryStream memoryStream )
                {
                    return memoryStream.ToArray();
                }

                using ( memoryStream = new MemoryStream() )
                {
                    stream.CopyTo( memoryStream );
                    return memoryStream.ToArray();
                }
            }
        }
    }
}