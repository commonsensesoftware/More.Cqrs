CREATE FUNCTION [Events].[GetEventName]
(
 @name NVARCHAR(256)
,@qualified BIT = 0
)
RETURNS NVARCHAR(256)
AS
BEGIN
	SET @name = SUBSTRING( @name, 0, CHARINDEX( ',', @name ) );
	
	IF ( COALESCE( @qualified, 0 ) = 0 )
	BEGIN
		SELECT TOP(1)
			@name = value
		FROM
			( SELECT
				 ROW_NUMBER() OVER( ORDER BY ( SELECT 1 ) ) row
				,value
			  FROM
				 STRING_SPLIT( @name, '.' )
			) array
		ORDER BY
			row DESC;
	END;

	RETURN @name;
END