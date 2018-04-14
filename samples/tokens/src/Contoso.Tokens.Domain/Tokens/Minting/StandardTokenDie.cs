namespace Contoso.Domain.Tokens.Minting
{
    using System;
    using System.Linq;
    using System.Security;
    using static System.BitConverter;

    public class StandardTokenDie : ITokenDie
    {
        readonly Random random = new Random();
        readonly byte[] salt;
        readonly TokenIdentifierGenerator generator;
        readonly ITokenSecurity security;

        public StandardTokenDie( Guid mintRequestId, long startOfSequence, ITokenSecurity security )
        {
            salt = mintRequestId.ToByteArray();
            generator = new TokenIdentifierGenerator( mintRequestId, startOfSequence );
            this.security = security;
        }

        public void Strike( TokenPlanchet planchet )
        {
            using ( var fiveByFive = Generate5x5UsingFakeAlgorithm() )
            {
                planchet.Id = generator.Next();
                planchet.Code = security.EncryptAsBase64( fiveByFive, salt );
                planchet.Hash = security.HashAsBase64( fiveByFive );
            }
        }

        SecureString Generate5x5UsingFakeAlgorithm()
        {
            var high = random.Next( 0x10000000, int.MaxValue );
            var mid = random.Next( 0, 0xF );
            var low = DateTime.UtcNow.Ticks;
            var hex = GetBytes( high ).Select( b => b.ToString( "X2" ) ).Union(
                      GetBytes( mid ).Take( 1 ).Select( b => b.ToString( "X" ) ) ).Union(
                      GetBytes( low ).Select( b => b.ToString( "X2" ) ) ).SelectMany( c => c );
            var fiveByFive = new SecureString();

            foreach ( var @char in hex )
            {
                fiveByFive.AppendChar( @char );
            }

            fiveByFive.MakeReadOnly();

            return fiveByFive;
        }
    }
}