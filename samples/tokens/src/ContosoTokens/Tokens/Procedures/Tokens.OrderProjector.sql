CREATE PROCEDURE [Tokens].[OrderProjector]
	 @type NVARCHAR(256)
	,@json NVARCHAR(MAX)
AS
SET NOCOUNT ON;

DECLARE @event NVARCHAR(256) = Events.GetEventName( @type, 0 );
DECLARE @id UNIQUEIDENTIFIER
       ,@tokenId VARCHAR(50)
	   ,@version INT = CAST( JSON_VALUE( @json, '$.version' ) AS INT );

IF ( @event = N'OrderPlaced' )
BEGIN
    SET @id = CAST( JSON_VALUE( @json, '$.aggregateId' ) AS UNIQUEIDENTIFIER );
	INSERT INTO Tokens.[Order]
	VALUES
	(
		 @id
		,@version
		,JSON_VALUE( @json, '$.catalogId' )
		,JSON_VALUE( @json, '$.billingAccountId' )
		,CAST( JSON_VALUE( @json, '$.quantity' ) AS INT )
		,0
	);
END;
ELSE IF ( @event = N'TokenReserved' )
BEGIN
    SET @id = CAST( JSON_VALUE( @json, '$.orderId' ) AS UNIQUEIDENTIFIER );

	-- short circuit if the order is already canceled
	IF ( ( SELECT State FROM Tokens.[Order] WHERE OrderId = @id ) = 3 )
	BEGIN
		RETURN 0;
	END;

	UPDATE Tokens.[Order] SET State = 1 WHERE OrderId = @id;

    SET @tokenId = JSON_VALUE( @json, '$.aggregateId' );
	
	MERGE Tokens.OrderLineItem AS target
	USING ( SELECT @id, @tokenId ) AS source (OrderId, TokenId)
	ON target.OrderId = source.OrderId AND target.TokenId = source.TokenId
	WHEN NOT MATCHED THEN INSERT ( OrderId, TokenId ) VALUES( source.OrderId, source.TokenId );
END;
ELSE IF ( @event = N'TokenUnreserved' )
BEGIN
    SET @id = CAST( JSON_VALUE( @json, '$.orderId' ) AS UNIQUEIDENTIFIER );
    SET @tokenId = JSON_VALUE( @json, '$.aggregateId' );
	DELETE FROM Tokens.OrderLineItem WHERE OrderId = @id AND TokenId = @tokenId;
END;
ELSE IF ( @event = N'OrderFulfilled' )
BEGIN
    SET @id = CAST( JSON_VALUE( @json, '$.aggregateId' ) AS UNIQUEIDENTIFIER );
	UPDATE Tokens.[Order] SET State = 2, Version = @version WHERE OrderId = @id AND Version <= @version;
END;
ELSE IF ( @event = N'OrderCanceled' )
BEGIN
    SET @id = CAST( JSON_VALUE( @json, '$.aggregateId' ) AS UNIQUEIDENTIFIER );
	UPDATE Tokens.[Order] SET State = 3, Version = @version WHERE OrderId = @id AND Version <= @version;
	DELETE FROM Tokens.OrderLineItem WHERE OrderId = @id;
END;

RETURN 0;