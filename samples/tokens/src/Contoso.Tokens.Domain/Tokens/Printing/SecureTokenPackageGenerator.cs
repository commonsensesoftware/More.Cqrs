namespace Contoso.Domain.Tokens.Printing
{
    using Security;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using static System.IntPtr;
    using static System.Runtime.InteropServices.Marshal;
    using static System.Text.Encoding;
    using static TokenIdentifier;

    public class SecureTokenPackageGenerator
    {
        readonly ICertificateLocator certificateLocator;
        readonly ITokenSecurity tokenSecurity;

        public SecureTokenPackageGenerator( ICertificateLocator certificateLocator, ITokenSecurity tokenSecurity )
        {
            this.certificateLocator = certificateLocator;
            this.tokenSecurity = tokenSecurity;
        }

        public async Task<TokenPackage> CreatePackage( IEnumerable<TokenReference> tokens, string thumbprint )
        {
            var certificate = certificateLocator.LocateByThumbprint( thumbprint );
            //var publicKey = certificate.PublicKey.Key;

            // TODO: encrypt package file stream using certificate
            // TODO: compress package? zip? mime type = application/zip
            // REF: https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.x509certificate2.aspx

            using ( var package = new TokenPackage( "text/csv" ) )
            using ( var writer = new StreamWriter( package, ASCII ) )
            {
                foreach ( var token in tokens )
                {
                    var id = Parse( token.Id );
                    var salt = id.MintRequestId.ToByteArray();
                    var tokenCode = tokenSecurity.DecryptFromBase64( token.Code, token.Hash, salt );
                    var @string = Zero;
                    var fiveByFive = default( string );

                    try
                    {
                        @string = SecureStringToGlobalAllocAnsi( tokenCode );
                        fiveByFive = PtrToStringAnsi( @string );
                    }
                    finally
                    {
                        if ( @string != Zero )
                        {
                            ZeroFreeGlobalAllocAnsi( @string );
                        }
                    }

                    await writer.WriteAsync( token.Id );
                    await writer.WriteAsync( "," );
                    await writer.WriteLineAsync( fiveByFive );
                }

                await writer.FlushAsync();
                package.Position = 0L;
                return package;
            }
        }
    }
}