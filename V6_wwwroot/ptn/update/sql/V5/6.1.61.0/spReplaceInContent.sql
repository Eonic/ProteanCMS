IF OBJECT_ID('dbo.spReplaceInContent', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spReplaceInContent;
GO

CREATE PROCEDURE dbo.spReplaceInContent
    @SearchText    NVARCHAR(MAX),
    @ReplaceText   NVARCHAR(MAX),
    @CaseSensitive BIT = 0,
    @Preview       BIT = 1,
    @ContentKeys   NVARCHAR(MAX) = NULL   -- optional comma separated nContentKey list
AS
BEGIN
    SET NOCOUNT ON;

    IF @SearchText IS NULL OR LEN(@SearchText) = 0
    BEGIN
        RAISERROR('@SearchText must not be empty.', 16, 1);
        RETURN;
    END
    SET @ReplaceText = ISNULL(@ReplaceText, N'');

    DECLARE @Matches TABLE (nContentKey INT PRIMARY KEY, InBrief BIT, InDetail BIT);

    INSERT INTO @Matches (nContentKey, InBrief, InDetail)
    SELECT c.nContentKey,
           CASE WHEN (@CaseSensitive = 0 AND CHARINDEX(@SearchText, CONVERT(NVARCHAR(MAX), c.cContentXmlBrief) COLLATE Latin1_General_100_CI_AS) > 0)
                  OR (@CaseSensitive = 1 AND CHARINDEX(@SearchText, CONVERT(NVARCHAR(MAX), c.cContentXmlBrief) COLLATE Latin1_General_100_CS_AS) > 0)
                THEN 1 ELSE 0 END,
           CASE WHEN (@CaseSensitive = 0 AND CHARINDEX(@SearchText, CONVERT(NVARCHAR(MAX), c.cContentXmlDetail) COLLATE Latin1_General_100_CI_AS) > 0)
                  OR (@CaseSensitive = 1 AND CHARINDEX(@SearchText, CONVERT(NVARCHAR(MAX), c.cContentXmlDetail) COLLATE Latin1_General_100_CS_AS) > 0)
                THEN 1 ELSE 0 END
    FROM dbo.tblContent c
    WHERE (@ContentKeys IS NULL
           OR c.nContentKey IN (SELECT TRY_CONVERT(INT, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@ContentKeys, ',')));

    DELETE FROM @Matches WHERE InBrief = 0 AND InDetail = 0;

    IF @Preview = 1
    BEGIN
        SELECT c.nContentKey, c.cContentSchemaName, c.cContentName, m.InBrief, m.InDetail
        FROM @Matches m
        JOIN dbo.tblContent c ON c.nContentKey = m.nContentKey
        ORDER BY c.nContentKey;
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @CaseSensitive = 1
            UPDATE c
            SET cContentXmlBrief = CASE WHEN m.InBrief = 1
                    THEN REPLACE(CONVERT(NVARCHAR(MAX), c.cContentXmlBrief) COLLATE Latin1_General_100_CS_AS, @SearchText, @ReplaceText)
                    ELSE CONVERT(NVARCHAR(MAX), c.cContentXmlBrief) END,
                cContentXmlDetail = CASE WHEN m.InDetail = 1
                    THEN REPLACE(CONVERT(NVARCHAR(MAX), c.cContentXmlDetail) COLLATE Latin1_General_100_CS_AS, @SearchText, @ReplaceText)
                    ELSE CONVERT(NVARCHAR(MAX), c.cContentXmlDetail) END
            FROM dbo.tblContent c
            JOIN @Matches m ON m.nContentKey = c.nContentKey;
        ELSE
            UPDATE c
            SET cContentXmlBrief = CASE WHEN m.InBrief = 1
                    THEN REPLACE(CONVERT(NVARCHAR(MAX), c.cContentXmlBrief) COLLATE Latin1_General_100_CI_AS, @SearchText, @ReplaceText)
                    ELSE CONVERT(NVARCHAR(MAX), c.cContentXmlBrief) END,
                cContentXmlDetail = CASE WHEN m.InDetail = 1
                    THEN REPLACE(CONVERT(NVARCHAR(MAX), c.cContentXmlDetail) COLLATE Latin1_General_100_CI_AS, @SearchText, @ReplaceText)
                    ELSE CONVERT(NVARCHAR(MAX), c.cContentXmlDetail) END
            FROM dbo.tblContent c
            JOIN @Matches m ON m.nContentKey = c.nContentKey;

        SELECT @@ROWCOUNT AS RowsUpdated;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO