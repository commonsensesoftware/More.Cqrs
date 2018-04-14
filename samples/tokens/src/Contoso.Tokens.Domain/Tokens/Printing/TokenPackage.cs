namespace Contoso.Domain.Tokens.Printing
{
    using System.IO;

    public class TokenPackage : MemoryStream
    {
        public TokenPackage( string mediaType ) => MediaType = mediaType;

        public string MediaType { get; }
    }
}