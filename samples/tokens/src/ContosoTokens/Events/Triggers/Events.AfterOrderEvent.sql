CREATE TRIGGER [Events].[AfterOrderEvent]
ON [Events].[Order] AFTER INSERT
AS
BEGIN
	SET NOCOUNT ON
	
	DECLARE @type NVARCHAR(256)
		   ,@message VARBINARY(MAX)
		   ,@json NVARCHAR(MAX);
	DECLARE Enumerator CURSOR FAST_FORWARD FOR SELECT Type, Message FROM INSERTED;

	OPEN Enumerator;
	FETCH NEXT FROM Enumerator INTO @type, @message;
	
	WHILE ( @@FETCH_STATUS = 0 )
	BEGIN
		SET @json = CAST(CAST(@message AS VARCHAR(MAX)) AS NVARCHAR(MAX));
		EXECUTE Tokens.OrderProjector @type, @json;
		FETCH NEXT FROM Enumerator INTO @type, @message;
	END;
	
	CLOSE Enumerator;
	DEALLOCATE Enumerator;
END