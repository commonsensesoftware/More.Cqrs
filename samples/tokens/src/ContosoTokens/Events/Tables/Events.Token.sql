CREATE TABLE [Events].[Token]
(
 AggregateId NVARCHAR(128) NOT NULL
,Version INT NOT NULL
,Sequence INT NOT NULL
,RecordedOn DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
,Type NVARCHAR(256) NOT NULL
,Revision INT NOT NULL DEFAULT(1)
,Message VARBINARY(MAX) NOT NULL
-- note: this is normally not needed, but given that minting could yield hundreds of thousands
-- millions, or even billions of tokens, these computed columns are used to optimize queries
,MintRequestId AS CAST(JSON_VALUE(CAST(Message AS VARCHAR(MAX)), '$.mintRequestId' ) AS UNIQUEIDENTIFIER)
,SequenceNumber AS CAST(JSON_VALUE(CAST(Message AS VARCHAR(MAX)), '$.sequenceNumber' ) AS INT)
,CONSTRAINT PK_Events_Token PRIMARY KEY( AggregateId, Version, Sequence )
,CONSTRAINT CK_Events_Token_Version CHECK( Version >= 0 )
,CONSTRAINT CK_Events_Token_Sequence CHECK( Sequence >= 0 )
);