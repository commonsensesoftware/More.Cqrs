namespace Contoso.Domain.Tokens.Minting
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay( "Id = {Id}, Hash = {Hash}, Code = {Code}" )]
    public class TokenPlanchet
    {
        public string Id { get; set; }

        public string Hash { get; set; }

        public string Code { get; set; }
    }
}