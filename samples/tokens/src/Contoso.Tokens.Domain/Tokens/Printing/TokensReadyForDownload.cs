namespace Contoso.Domain.Tokens.Printing
{
    using System;
    using More.Domain.Events;
    using System.Collections.Generic;

    public class TokensReadyForDownload : Event
    {
        // TODO: does this need to be an array for deserialization?

        public TokensReadyForDownload( Guid aggregateId, IReadOnlyList<byte> encryptedFileContents )
        {
            AggregateId = aggregateId;
            EncryptedFileContents = encryptedFileContents;
        }

        public IReadOnlyList<byte> EncryptedFileContents { get; }
    }
}