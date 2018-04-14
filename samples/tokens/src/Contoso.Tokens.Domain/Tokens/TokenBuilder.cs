namespace Contoso.Domain.Tokens
{
    using System;

    public class TokenBuilder
    {
        string id;
        Guid mintRequestId;
        string code;
        string hash;
        string catalogId;
        long sequenceNumber;

        public TokenBuilder HasId( string value )
        {
            id = value;
            return this;
        }

        public TokenBuilder HasMintRequestId( Guid value )
        {
            mintRequestId = value;
            return this;
        }

        public TokenBuilder HasCode( string value )
        {
            code = value;
            return this;
        }

        public TokenBuilder HasHash( string value )
        {
            hash = value;
            return this;
        }

        public TokenBuilder HasCatalogId( string value )
        {
            catalogId = value;
            return this;
        }

        public TokenBuilder HasSequenceNumber( long value )
        {
            sequenceNumber = value;
            return this;
        }

        public Token CreateToken()
        {
            return new Token(
                id,
                mintRequestId,
                code,
                hash,
                catalogId,
                sequenceNumber );
        }
    }
}