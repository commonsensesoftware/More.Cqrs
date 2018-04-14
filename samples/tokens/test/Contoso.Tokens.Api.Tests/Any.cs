namespace Contoso.Services
{
    using System;
    using System.Security.Cryptography;
    using static System.BitConverter;
    using static System.Int32;
    using static System.Math;
    using static System.Security.Cryptography.RandomNumberGenerator;

    static class Any
    {
        static readonly Random random = new PseudoRandom();

        internal static string CatalogId => random.Next( 10000, 100000 ).ToString();

        sealed class PseudoRandom : Random
        {
            const double Denominator = UInt32.MaxValue + 1d;
            readonly RandomNumberGenerator rng = Create();

            public override void NextBytes( byte[] data ) => rng.GetBytes( data );

            protected override double Sample()
            {
                var number = new byte[4];
                rng.GetBytes( number );
                return ToUInt32( number, 0 ) / Denominator;
            }

            public override int Next() => Next( 0, MaxValue );

            public override int Next( int maxValue ) => Next( 0, maxValue );

            public override int Next( int minValue, int maxValue ) => (int) Floor( Sample() * ( maxValue - minValue ) ) + minValue;
        }
    }
}