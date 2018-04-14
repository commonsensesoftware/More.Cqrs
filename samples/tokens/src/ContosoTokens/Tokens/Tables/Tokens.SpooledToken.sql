CREATE TABLE [Tokens].[SpooledToken]
(
 PrintJobId UNIQUEIDENTIFIER NOT NULL
,TokenId VARCHAR(50) NOT NULL
,CONSTRAINT PK_Tokens_SpooledToken PRIMARY KEY( PrintJobId, TokenId )
,CONSTRAINT FK_Tokens_SpooledToken_PrintJob FOREIGN KEY ( PrintJobId )
	REFERENCES [Tokens].[PrintJob]( PrintJobId ) ON DELETE CASCADE
,CONSTRAINT FK_Tokens_SpooledToken_Token FOREIGN KEY ( TokenId )
	REFERENCES [Tokens].[Token]( TokenId )
);