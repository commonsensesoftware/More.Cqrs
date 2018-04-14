CREATE TRIGGER [Messaging].[AfterInsertIntoMessageQueue] ON [Messaging].[MessageQueue] FOR INSERT AS
BEGIN
SET NOCOUNT ON;

INSERT INTO [Messaging].[SubscriptionQueue]
SELECT
     s.SubscriptionId
    ,i.MessageId
    ,i.EnqueueTime
    ,0 -- DequeueAttempts
    ,i.DueTime
    ,i.Type
    ,i.Revision
    ,i.Message
FROM
    [Messaging].[Subscription] s
    ,INSERTED i;
END;