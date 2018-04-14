CREATE PROCEDURE [Tokens].[PrintJobProjector]
	 @type NVARCHAR(256)
	,@json NVARCHAR(MAX)
AS
SET NOCOUNT ON;

DECLARE @event NVARCHAR(256) = Events.GetEventName( @type, 0 );
DECLARE @id UNIQUEIDENTIFIER = CAST( JSON_VALUE( @json, '$.aggregateId' ) AS UNIQUEIDENTIFIER )
	   ,@version INT = CAST( JSON_VALUE( @json, '$.version' ) AS INT );

IF ( @event = N'Print' )
BEGIN
	INSERT INTO Tokens.PrintJob
	VALUES
	(
		 @id
		,@version
		,JSON_VALUE( @json, '$.catalogId' )
		,JSON_VALUE( @json, '$.billingAccountId' )
		,CAST( JSON_VALUE( @json, '$.quantity' ) AS INT )
		,0
		,JSON_VALUE( @json, '$.certificateThumbprint' )
	);
END;
ELSE IF ( @event = N'TokenSpooled' )
BEGIN
	-- short circuit if the print job is already canceled
	IF ( ( SELECT State FROM Tokens.PrintJob WHERE PrintJobId = @id ) = 5 )
	BEGIN
		RETURN 0;
	END;

	MERGE Tokens.SpooledToken AS target
	USING ( SELECT @id, JSON_VALUE( @json, '$.tokenId' ) ) AS source (PrintJobId, TokenId)
	ON target.PrintJobId = source.PrintJobId AND target.TokenId = source.TokenId
	WHEN NOT MATCHED THEN INSERT ( PrintJobId, TokenId ) VALUES( source.PrintJobId, source.TokenId );
END;
ELSE IF ( @event = N'TokenUnspooled' )
BEGIN
	DELETE FROM Tokens.SpooledToken WHERE PrintJobId = @id AND TokenId = JSON_VALUE( @json, '$.tokenId' );
END;
ELSE IF ( @event = N'PrintJobSpooled' )
BEGIN
	UPDATE Tokens.PrintJob SET State = 1, Version = @version WHERE PrintJobId = @id AND Version <= @version;
END;
ELSE IF ( @event = N'TokenPrinted' )
BEGIN
	UPDATE Tokens.PrintJob SET State = 2, Version = @version WHERE PrintJobId = @id AND Version <= @version;
END;
ELSE IF ( @event = N'PrintJobCompleted' )
BEGIN
	UPDATE Tokens.PrintJob SET State = 3, Version = @version WHERE PrintJobId = @id AND Version <= @version;
END;
ELSE IF ( @event = N'PrintJobCanceled' )
BEGIN
	UPDATE Tokens.PrintJob SET State = 4, Version = @version WHERE PrintJobId = @id AND Version <= @version;
	DELETE FROM Tokens.SpooledToken WHERE PrintJobId = @id;
END;

RETURN 0;