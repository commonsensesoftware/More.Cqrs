CREATE PROCEDURE [Tokens].[Transfer]
 @OrderId UNIQUEIDENTIFIER
,@BillingAccountId VARCHAR(25)
,@CatalogId VARCHAR(25)
,@Quantity INT
,@ActivateImmediately BIT
,@CorrelationId VARCHAR(50)

AS
SET NOCOUNT ON;

DECLARE @messages TABLE( Type NVARCHAR(256), Revision INT, Message VARBINARY(MAX) );
DECLARE @aggregateId NVARCHAR(128)
       ,@version INT
       ,@code VARCHAR(50)
       ,@hash VARCHAR(50)
       ,@recordedOn DATETIME2 = GETUTCDATE();

-- enumerate all circulated tokens matching the requested catalog item
DECLARE Enumerator CURSOR FAST_FORWARD FOR
        SELECT
             TokenId
            ,Version
            ,Code
            ,Hash
        FROM
            [Tokens].[Token]
        WHERE
            CatalogId = @CatalogId
            AND State = 1;

OPEN Enumerator;
FETCH NEXT FROM Enumerator INTO @aggregateId, @version, @code, @hash;

BEGIN TRANSACTION;

WHILE ( @Quantity > 0 AND @@FETCH_STATUS = 0 )
BEGIN

    BEGIN TRY

        -- insert the reservation event and enqueue the corresponing event
        INSERT INTO [Events].[Token]
        OUTPUT inserted.Type, inserted.Revision, inserted.Message INTO @messages
        SELECT
             @aggregateId
            ,@version + 1
            ,0 -- sequence
            ,@recordedOn
            ,N'Contoso.Domain.Tokens.TokenReserved, Contoso.Tokens.Domain'
            ,1 -- revision
            ,CAST(CAST((SELECT
                 @OrderId orderId
                ,@BillingAccountId billingAccountId
                ,@code code
                ,@hash hash
                ,@aggregateId aggregateId
                ,(@version + 1) version
                ,0 sequence
                ,1 revision
                ,@CorrelationId correlationId
                FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS VARCHAR(MAX)) AS VARBINARY(MAX));

        IF ( @ActivateImmediately = 1 )
        BEGIN

            -- insert the activation event and enqueue the corresponing event
            INSERT INTO [Events].[Token]
            OUTPUT inserted.Type, inserted.Revision, inserted.Message INTO @messages
            SELECT
                 @aggregateId
                ,@version + 2
                ,0 -- sequence
                ,@recordedOn
                ,N'Contoso.Domain.Tokens.TokenActivated, Contoso.Tokens.Domain'
                ,1 -- revision
                ,CAST(CAST((SELECT
                     @OrderId orderId
                    ,@BillingAccountId billingAccountId
                    ,@aggregateId aggregateId
                    ,(@version + 2) version
                    ,0 sequence
                    ,1 revision
                    ,@CorrelationId correlationId
                    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS VARCHAR(MAX)) AS VARBINARY(MAX));

        END;

        SET @Quantity -= 1;

    END TRY
    BEGIN CATCH

        -- TODO: what should we do when an error other than a primary violation occurs
        --IF ( ERROR_NUMBER() <> 2627 )
        --BEGIN
                 
        --END

    END CATCH;
        
    FETCH NEXT FROM Enumerator INTO @aggregateId, @version, @code, @hash;

END;

CLOSE Enumerator;
DEALLOCATE Enumerator;

-- commit and report success if the reservation was successful
IF ( @Quantity = 0 )
BEGIN

    INSERT INTO [Messaging].[MessageQueue] ( Type, Revision, Message )
    SELECT Type, Revision, Message FROM @messages;

    COMMIT TRANSACTION;
    RETURN 0;
END;

ROLLBACK TRANSACTION;
RETURN -1;