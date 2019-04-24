CREATE PROCEDURE [Tokens].[ReverseTransfer]
 @OrderId UNIQUEIDENTIFIER
,@CorrelationId VARCHAR(50)

AS
SET NOCOUNT ON;

DECLARE @messages TABLE( Type NVARCHAR(256), Revision INT, Message VARBINARY(MAX) );
DECLARE @recordedOn DATETIME2 = GETUTCDATE();

-- insert the unreservation event and enqueue the corresponding event
INSERT INTO [Events].[Token]
OUTPUT inserted.Type, inserted.Revision, inserted.Message INTO @messages
SELECT
     t.AggregateId
    ,t.Version + 1
    ,0 -- sequence
    ,@recordedOn
    ,N'Contoso.Domain.Tokens.TokenUnreserved, Contoso.Tokens.Domain'
    ,1 -- revision
    ,CAST(CAST((SELECT
         @OrderId orderId
        ,TokenId aggregateId
        ,(t.Version + 1) version
        ,0 sequence
        ,1 revision
        ,@CorrelationId correlationId
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS VARCHAR(MAX)) AS VARBINARY(MAX))
FROM
    [Tokens].[OrderLineItem] oli
    INNER JOIN (
        SELECT
             AggregateId
            ,MAX(Version) Version
        FROM
            [Events].[Token]
        GROUP BY
            AggregateId
    ) t ON t.AggregateId = oli.TokenId
WHERE
    oli.OrderId = @OrderId;

INSERT INTO [Messaging].[MessageQueue] ( Type, Revision, Message )
SELECT Type, Revision, Message FROM @messages;

RETURN 0;