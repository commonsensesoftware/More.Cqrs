// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;

    sealed class Sha1ForRfc4122
    {
        long length;
        int position;
        uint[] workspace = new uint[85];

        internal Sha1ForRfc4122()
        {
            workspace[80] = 0x67452301;
            workspace[81] = 0xEFCDAB89;
            workspace[82] = 0x98BADCFE;
            workspace[83] = 0x10325476;
            workspace[84] = 0xC3D2E1F0;
        }

        public void Append( byte @byte )
        {
            var i = position >> 2;

            workspace[i] = ( workspace[i] << 8 ) | @byte;

            if ( ++position == 64 )
            {
                Drain();
            }
        }

        public void Append( byte[] bytes )
        {
            foreach ( var @byte in bytes )
            {
                Append( @byte );
            }
        }

        public void Finish( byte[] bytes )
        {
            var l = length + ( 8 * position );

            Append( 0x80 );

            while ( position != 56 )
            {
                Append( 0x00 );
            }

            unchecked
            {
                Append( (byte) ( l >> 56 ) );
                Append( (byte) ( l >> 48 ) );
                Append( (byte) ( l >> 40 ) );
                Append( (byte) ( l >> 32 ) );
                Append( (byte) ( l >> 24 ) );
                Append( (byte) ( l >> 16 ) );
                Append( (byte) ( l >> 8 ) );
                Append( (byte) l );

                var end = bytes.Length < 20 ? bytes.Length : 20;

                for ( var i = 0; i != end; i++ )
                {
                    var temp = workspace[80 + ( i / 4 )];
                    bytes[i] = (byte) ( temp >> 24 );
                    workspace[80 + ( i / 4 )] = temp << 8;
                }
            }
        }

        void Drain()
        {
            for ( var i = 16; i != 80; i++ )
            {
                workspace[i] = Rol1( workspace[i - 3] ^ workspace[i - 8] ^ workspace[i - 14] ^ workspace[i - 16] );
            }

            unchecked
            {
                var a = workspace[80];
                var b = workspace[81];
                var c = workspace[82];
                var d = workspace[83];
                var e = workspace[84];

                for ( var i = 0; i != 20; i++ )
                {
                    const uint k = 0x5A827999U;
                    var f = ( b & c ) | ( ( ~b ) & d );
                    var temp = Rol5( a ) + f + e + k + workspace[i];
                    e = d;
                    d = c;
                    c = Rol30( b );
                    b = a;
                    a = temp;
                }

                for ( var i = 20; i != 40; i++ )
                {
                    const uint k = 0x6ED9EBA1U;
                    var f = b ^ c ^ d;
                    var temp = Rol5( a ) + f + e + k + workspace[i];
                    e = d;
                    d = c;
                    c = Rol30( b );
                    b = a;
                    a = temp;
                }

                for ( var i = 40; i != 60; i++ )
                {
                    const uint k = 0x8F1BBCDCU;
                    var f = ( b & c ) | ( b & d ) | ( c & d );
                    var temp = Rol5( a ) + f + e + k + workspace[i];
                    e = d;
                    d = c;
                    c = Rol30( b );
                    b = a;
                    a = temp;
                }

                for ( var i = 60; i != 80; i++ )
                {
                    var f = b ^ c ^ d;
                    const uint k = 0xCA62C1D6U;
                    var temp = Rol5( a ) + f + e + k + workspace[i];
                    e = d;
                    d = c;
                    c = Rol30( b );
                    b = a;
                    a = temp;
                }

                workspace[80] += a;
                workspace[81] += b;
                workspace[82] += c;
                workspace[83] += d;
                workspace[84] += e;
            }

            length += 512L;
            position = 0;
        }

        static uint Rol1( uint input ) => ( input << 1 ) | ( input >> 31 );

        static uint Rol5( uint input ) => ( input << 5 ) | ( input >> 27 );

        static uint Rol30( uint input ) => ( input << 30 ) | ( input >> 2 );
    }
}