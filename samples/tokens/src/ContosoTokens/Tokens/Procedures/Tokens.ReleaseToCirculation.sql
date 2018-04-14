CREATE PROCEDURE [Tokens].[ReleaseToCirculation]
 @MintRequestId UNIQUEIDENTIFIER
,@CorrelationId VARCHAR(50)

AS
SET NOCOUNT ON;

-- determine the total quantity from the number of mint jobs generated in the mint request
DECLARE @quantity BIGINT = (
    SELECT
	    ( SELECT SUM( [Count] ) FROM OPENJSON( CAST(Message AS VARCHAR(MAX)), '$.mintJobs' ) WITH ( [Count] BIGINT '$.count' ) )
    FROM
	    [Events].[MintRequest]
    WHERE
        AggregateId = @MintRequestId
	    AND Version = 0
        AND Type = N'Contoso.Domain.Tokens.Minting.MintRequested, Contoso.Tokens.Domain' );

DECLARE @batchSize BIGINT = 1000000;
DECLARE @start BIGINT = 1
       ,@end BIGINT = @batchSize
       ,@recordedOn DATETIME2 = GETUTCDATE();

-- note: this is ATYPICAL for event generation; however, given the potential for a very large number
-- of operations, we do all the work directly in the database for this step. we also batch the
-- generated events to ease the pressure on the transaction log. the addition of the events is
-- idempotent so that this procedure can be run multiple times if needed.
WHILE ( @start <= @quantity )
BEGIN

    BEGIN TRANSACTION;

    MERGE [Events].[Token] AS target
    USING (
        SELECT
	         AggregateId
	        ,Version + 1
	        ,0 -- sequence
        FROM
	        [Events].[Token]
        WHERE
            MintRequestId = @MintRequestId
	        AND Version = 0
            AND SequenceNumber BETWEEN @start AND @end
        ) AS source ( AggregateId, Version, Sequence )
    ON target.AggregateId = source.AggregateId AND target.Version = source.Version AND target.Sequence = source.Sequence
    WHEN NOT MATCHED THEN
        INSERT ( AggregateId, Version, Sequence, RecordedOn, Type, Revision, Message )
        VALUES
        (
             source.AggregateId
            ,source.Version
            ,source.Sequence
            ,@recordedOn
            ,N'Contoso.Domain.Tokens.TokenCirculated, Contoso.Tokens.Domain'
            ,1 -- revision
            ,CAST(CAST((SELECT source.aggregateId, source.version, source.sequence, 1 revision, @CorrelationId correlationId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS VARCHAR(MAX)) AS VARBINARY(MAX))
        );

    COMMIT TRANSACTION;

    SET @start = @end + 1;
    SET @end = @end + @batchSize;

END;

RETURN 0;