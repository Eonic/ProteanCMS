IF OBJECT_ID('dbo.spFindInContent', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spFindInContent;
GO

CREATE PROCEDURE dbo.spFindInContent
    @SearchText    NVARCHAR(MAX),
    @CaseSensitive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @SearchText IS NULL OR LEN(@SearchText) = 0
    BEGIN
        RAISERROR('@SearchText must not be empty.', 16, 1);
        RETURN;
    END

    SELECT
        c.nContentKey,
        c.cContentSchemaName,
        c.cContentName,
        CASE WHEN (@CaseSensitive = 0 AND CHARINDEX(@SearchText, CONVERT(NVARCHAR(MAX), c.cContentXmlBrief) COLLATE Latin1_General_100_CI_AS) > 0)
               OR (@CaseSensitive = 1 AND CHARINDEX(@SearchText, CONVERT(NVARCHAR(MAX), c.cContentXmlBrief) COLLATE Latin1_General_100_CS_AS) > 0)
             THEN 1 ELSE 0 END AS InBrief,
        CASE WHEN (@CaseSensitive = 0 AND CHARINDEX(@SearchText, CONVERT(NVARCHAR(MAX), c.cContentXmlDetail) COLLATE Latin1_General_100_CI_AS) > 0)
               OR (@CaseSensitive = 1 AND CHARINDEX(@SearchText, CONVERT(NVARCHAR(MAX), c.cContentXmlDetail) COLLATE Latin1_General_100_CS_AS) > 0)
             THEN 1 ELSE 0 END AS InDetail
    FROM dbo.tblContent c
    WHERE (@CaseSensitive = 0 AND (
                CHARINDEX(@SearchText, CONVERT(NVARCHAR(MAX), c.cContentXmlBrief)  COLLATE Latin1_General_100_CI_AS) > 0
             OR CHARINDEX(@SearchText, CONVERT(NVARCHAR(MAX), c.cContentXmlDetail) COLLATE Latin1_General_100_CI_AS) > 0))
       OR (@CaseSensitive = 1 AND (
                CHARINDEX(@SearchText, CONVERT(NVARCHAR(MAX), c.cContentXmlBrief)  COLLATE Latin1_General_100_CS_AS) > 0
             OR CHARINDEX(@SearchText, CONVERT(NVARCHAR(MAX), c.cContentXmlDetail) COLLATE Latin1_General_100_CS_AS) > 0))
    ORDER BY c.nContentKey;
END
GO