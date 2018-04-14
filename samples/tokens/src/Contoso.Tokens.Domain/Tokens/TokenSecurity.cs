namespace Contoso.Domain.Tokens
{
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Cryptography;
    using static System.Convert;
    using static System.IntPtr;
    using static System.Runtime.InteropServices.Marshal;
    using static System.Security.Cryptography.CryptoStreamMode;
    using static System.Text.Encoding;

    public class TokenSecurity : ITokenSecurity
    {
        public string EncryptAsBase64( SecureString tokenCode, byte[] salt )
        {
            var @string = SecureStringToGlobalAllocAnsi( tokenCode );

            try
            {
                var fiveByFive = PtrToStringAnsi( @string );
                var password = default( string );

                using ( var sha = SHA256.Create() )
                {
                    password = ToBase64String( sha.ComputeHash( ASCII.GetBytes( fiveByFive ) ) );
                }

                using ( var deriveBytes = new Rfc2898DeriveBytes( password, salt ) )
                using ( var aes = Aes.Create() )
                {
                    aes.Key = deriveBytes.GetBytes( aes.KeySize >> 3 );
                    aes.IV = deriveBytes.GetBytes( aes.BlockSize >> 3 );

                    using ( var buffer = new MemoryStream() )
                    using ( var cipher = new CryptoStream( buffer, aes.CreateEncryptor(), Write ) )
                    {
                        var bytes = ASCII.GetBytes( fiveByFive );

                        cipher.Write( bytes, 0, bytes.Length );
                        cipher.FlushFinalBlock();

                        return ToBase64String( buffer.ToArray() );
                    }
                }
            }
            finally
            {
                if ( @string != Zero )
                {
                    ZeroFreeGlobalAllocAnsi( @string );
                }
            }
        }

        public SecureString DecryptFromBase64( string tokenCode, string tokenHash, byte[] salt )
        {
            using ( var deriveBytes = new Rfc2898DeriveBytes( tokenHash, salt ) )
            using ( var aes = Aes.Create() )
            {
                aes.Key = deriveBytes.GetBytes( aes.KeySize >> 3 );
                aes.IV = deriveBytes.GetBytes( aes.BlockSize >> 3 );

                using ( var buffer = new MemoryStream( FromBase64String( tokenCode ) ) )
                using ( var cipher = new CryptoStream( buffer, aes.CreateDecryptor(), Read ) )
                using ( var reader = new StreamReader( cipher ) )
                {
                    var fiveByFive = new SecureString();

                    foreach ( var @char in reader.ReadToEnd() )
                    {
                        fiveByFive.AppendChar( @char );
                    }

                    fiveByFive.MakeReadOnly();

                    return fiveByFive;
                }
            }
        }

        public string HashAsBase64( SecureString tokenCode )
        {
            var @string = SecureStringToGlobalAllocAnsi( tokenCode );

            try
            {
                var fiveByFive = ASCII.GetBytes( PtrToStringAnsi( @string ) );

                using ( var sha = SHA256.Create() )
                {
                    return ToBase64String( sha.ComputeHash( fiveByFive ) );
                }
            }
            finally
            {
                if ( @string != Zero )
                {
                    ZeroFreeGlobalAllocAnsi( @string );
                }
            }
        }
    }
}