CREATE PROCEDURE [Tokens].[TokenProjector]
	 @type NVARCHAR(256)
	,@json NVARCHAR(MAX)
AS
SET NOCOUNT ON;

DECLARE @event NVARCHAR(256) = Events.GetEventName( @type, 0 );
DECLARE @id VARCHAR(50) = JSON_VALUE( @json, '$.aggregateId' )
	   ,@version INT = CAST( JSON_VALUE( @json, '$.version' ) AS INT );

IF ( @event = N'TokenMinted' )
BEGIN
	INSERT INTO Tokens.Token
	VALUES
	(
		 @id
		,@version
		,JSON_VALUE( @json, '$.catalogId' )
		,NULL
		,NULL
		,JSON_VALUE( @json, '$.code' )
		,JSON_VALUE( @json, '$.hash' )
		,0
	);
END;
ELSE IF ( @event = N'TokenCirculated' )
BEGIN
	UPDATE
		Tokens.Token
	SET
		 State = 1
		,Version = @version
	WHERE
		TokenId = @id
		AND Version <= @version;
END;
ELSE IF ( @event = N'TokenReserved' )
BEGIN
	UPDATE
		Tokens.Token
	SET
		 State = 2
		,Version = @version
		,ReservedByBillingAccountId = JSON_VALUE( @json, '$.billingAccountId' )
	WHERE
		TokenId = @id
		AND Version <= @version;
END;
ELSE IF ( @event = N'TokenActivated' )
BEGIN
	UPDATE
		Tokens.Token
	SET
		 State = 3
		,Version = @version
		,ReservedByBillingAccountId = JSON_VALUE( @json, '$.billingAccountId' )
	WHERE
		TokenId = @id
		AND Version <= @version;
END;
ELSE IF ( @event = N'TokenRedeemed' )
BEGIN
	UPDATE
		Tokens.Token
	SET
		 State = 4
		,Version = @version
		,RedeemedByAccountId = JSON_VALUE( @json, '$.accountId' )
	WHERE
		TokenId = @id
		AND Version <= @version;
END;
ELSE IF ( @event = N'TokenVoided' )
BEGIN
	UPDATE
		Tokens.Token
	SET
		 State = 5
		,Version = @version
	WHERE
		TokenId = @id
		AND Version <= @version;
END;
ELSE IF ( @event = N'TokenDeactivated' )
BEGIN
	UPDATE
		Tokens.Token
	SET
		 State = CASE WHEN ReservedByBillingAccountId IS NULL THEN 1 ELSE 2 END
		,Version = @version
	WHERE
		TokenId = @id
		AND Version <= @version;
END;
ELSE IF ( @event = N'TokenUnreserved' )
BEGIN
	UPDATE
		Tokens.Token
	SET
		 State = 1
		,Version = @version
		,ReservedByBillingAccountId = NULL
	WHERE
		TokenId = @id
		AND Version <= @version;
END;

RETURN 0;