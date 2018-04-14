namespace Contoso.Domain
{
    using More.Domain;
    using System;
    using System.Threading;
    using static System.Guid;
    using static System.Int32;

    static class Any
    {
        static readonly ThreadLocal<Random> random = new ThreadLocal<Random>( () => new Random() );

        internal static string NumericString => random.Value.Next(10000, MaxValue).ToString();

        internal static Guid Guid => Uuid.NewSequentialId();

        internal static string IdempotencyToken => Checksum.AsBase64( NewGuid().ToByteArray() );

        internal static string CorrelationId => NewGuid().ToString();
    }
}