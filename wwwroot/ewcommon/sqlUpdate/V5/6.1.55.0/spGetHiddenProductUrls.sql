CREATE PROCEDURE spGetHiddenProductUrls
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  
        c.nContentKey,
        c.cContentSchemaName,
        c.cContentName
    FROM tblContent c
    JOIN tblAudit a ON a.nAuditKey = c.nAuditId
    WHERE c.cContentSchemaName = 'Product'
      AND a.nStatus = 0;  
END
GO