CREATE PROCEDURE [Tokens].[MintRequestProjector]
	 @type NVARCHAR(256)
	,@json NVARCHAR(MAX)
AS
SET NOCOUNT ON;

DECLARE @event NVARCHAR(256) = Events.GetEventName( @type, 0 );
DECLARE @id UNIQUEIDENTIFIER = CAST( JSON_VALUE( @json, '$.aggregateId' ) AS UNIQUEIDENTIFIER )
	   ,@version INT = CAST( JSON_VALUE( @json, '$.version' ) AS INT );

IF ( @event = N'MintRequested' )
BEGIN
	DECLARE @quanity BIGINT = ( SELECT SUM( [Count] ) FROM OPENJSON( @json, '$.mintJobs' ) WITH ( [Count] BIGINT '$.count' ) );
	INSERT INTO Tokens.MintRequest VALUES( @id, @version, JSON_VALUE( @json, '$.catalogId' ), @quanity, 0 );
END;
ELSE IF ( @event = N'Minted' )
BEGIN
	UPDATE Tokens.MintRequest SET State = 1, Version = @version WHERE MintRequestId = @id AND Version <= @version;
END;
ELSE IF ( @event = N'MintCanceling' )
BEGIN
	UPDATE Tokens.MintRequest SET State = 2, Version = @version WHERE MintRequestId = @id AND Version <= @version;
END;
ELSE IF ( @event = N'MintCanceled' )
BEGIN
	UPDATE Tokens.MintRequest SET State = 3, Version = @version WHERE MintRequestId = @id AND Version <= @version;
END;

RETURN 0;