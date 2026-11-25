IF OBJECT_ID('dbo.spFindIndexNearest', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spFindIndexNearest;
GO

CREATE PROCEDURE dbo.spFindIndexNearest
    @SchemaName NVARCHAR(255),
    @latitude FLOAT,
    @longitude FLOAT,
    @count int
AS
BEGIN
    SET NOCOUNT ON;

    -- Returns nContentKeys of the nearest indexed content, using the index definitions
    -- @SchemaName + '-Lat' and @SchemaName + '-Long' (values held as text or number), nearest first.
    SELECT TOP (@count) lat.nContentId AS nContentKey
    FROM tblContentIndex lat
    INNER JOIN tblContentIndexDef latDef
        ON latDef.nContentIndexDefKey = lat.nContentIndexDefinitionKey
        AND latDef.cDefinitionName = @SchemaName + '-Lat'
    INNER JOIN tblContentIndex lng
        ON lng.nContentId = lat.nContentId
    INNER JOIN tblContentIndexDef lngDef
        ON lngDef.nContentIndexDefKey = lng.nContentIndexDefinitionKey
        AND lngDef.cDefinitionName = @SchemaName + '-Long'
    INNER JOIN tblContent c
        ON c.nContentKey = lat.nContentId
    INNER JOIN tblAudit a
        ON a.nAuditKey = c.nAuditId AND a.nStatus = 1
    CROSS APPLY (SELECT
            COALESCE(TRY_CONVERT(FLOAT, lat.cTextValue), CONVERT(FLOAT, lat.nNumberValue)) AS la,
            COALESCE(TRY_CONVERT(FLOAT, lng.cTextValue), CONVERT(FLOAT, lng.nNumberValue)) AS lo) v
    WHERE v.la IS NOT NULL AND v.lo IS NOT NULL
    ORDER BY
        -- haversine (km)
        2 * 6371 * ASIN(SQRT(
            POWER(SIN(RADIANS(v.la - @latitude) / 2), 2) +
            COS(RADIANS(@latitude)) * COS(RADIANS(v.la)) * POWER(SIN(RADIANS(v.lo - @longitude) / 2), 2)
        )) ASC;

   
END
GO