namespace Contoso.Domain.Tokens
{
    using System;
    using System.Security;

    public interface ITokenSecurity
    {
        string EncryptAsBase64( SecureString tokenCode, byte[] salt );

        SecureString DecryptFromBase64( string tokenCode, string tokenHash, byte[] salt );

        string HashAsBase64( SecureString tokenCode );
    }
}
