CREATE TABLE [Messaging].[SubscriptionQueue]
(
 SubscriptionId UNIQUEIDENTIFIER NOT NULL
,MessageId UNIQUEIDENTIFIER NOT NULL
,EnqueueTime DATETIME2 NOT NULL
,DequeueAttempts INT NOT NULL DEFAULT(0)
,DueTime DATETIME2 NOT NULL
,Type NVARCHAR(256) NOT NULL
,Revision INT NOT NULL
,Message VARBINARY(MAX) NOT NULL
,CONSTRAINT PK_Messaging_SubscriptionQueue PRIMARY KEY NONCLUSTERED ( SubscriptionId, MessageId )
,CONSTRAINT FK_Messaging_SubscriptionQueue_Subscription FOREIGN KEY ( SubscriptionId )
    REFERENCES [Messaging].[Subscription] ( SubscriptionId ) ON DELETE CASCADE
);
GO

CREATE CLUSTERED INDEX IX_Messaging_SubscriptionQueue ON [Messaging].[SubscriptionQueue]( DueTime, DequeueAttempts );