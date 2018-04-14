CREATE TABLE [Events].[MintRequest]
(
 AggregateId UNIQUEIDENTIFIER NOT NULL
,Version INT NOT NULL
,Sequence INT NOT NULL
,RecordedOn DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
,Type NVARCHAR(256) NOT NULL
,Revision INT NOT NULL DEFAULT(1)
,Message VARBINARY(MAX) NOT NULL
,CONSTRAINT PK_Events_MintRequest PRIMARY KEY( AggregateId, Version, Sequence )
,CONSTRAINT CK_Events_MintRequest_Version CHECK( Version >= 0 )
,CONSTRAINT CK_Events_MintRequest_Sequence CHECK( Sequence >= 0 )
);