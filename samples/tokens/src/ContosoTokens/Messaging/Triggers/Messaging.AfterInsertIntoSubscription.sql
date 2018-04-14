CREATE TRIGGER [Messaging].[AfterInsertIntoSubscription] ON [Messaging].[Subscription] FOR INSERT AS
BEGIN
SET NOCOUNT ON;

INSERT INTO [Messaging].[SubscriptionQueue]
SELECT
     i.SubscriptionId
    ,q.MessageId
    ,q.EnqueueTime
    ,0 -- DequeueAttempts
    ,q.DueTime
    ,q.Type
    ,q.Revision
    ,q.Message
FROM
    INSERTED i
    INNER JOIN [Messaging].[MessageQueue] q ON q.EnqueueTime >= i.CreationTime;
END