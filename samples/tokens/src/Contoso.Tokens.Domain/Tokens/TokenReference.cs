namespace Contoso.Domain.Tokens
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay( "Id = {Id}, Version = {Version}, Hash = {Hash}, Code = {Code}" )]
    public class TokenReference
    {
        public TokenReference( string id, int version, string code, string hash )
        {
            Id = id;
            Version = version;
            Code = code;
            Hash = hash;
        }

        public string Id { get; }

        public string Code { get; }

        public string Hash { get; }

        public int Version { get; }
    }
}