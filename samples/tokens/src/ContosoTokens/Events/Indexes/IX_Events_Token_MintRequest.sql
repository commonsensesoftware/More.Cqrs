CREATE INDEX [IX_Events_Token_MintRequest]
ON [Events].[Token]( MintRequestId, Version, SequenceNumber )
INCLUDE( AggregateId, Revision );