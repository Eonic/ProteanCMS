CREATE PROCEDURE [dbo].[spUpdateMenuTitleandName]          

@pagetitleId int,
@metadescId int,
@pagetitle Nvarchar(max),
@metadescription Nvarchar(max),
@PageName Nvarchar(max),
@DisplayName Nvarchar(max),
@menuid int

AS    

BEGIN   

--update content table data
UPDATE tblContent SET cContentXmlBrief = CASE WHEN nContentKey = @pagetitleId THEN @pagetitle WHEN nContentKey = @metadescId THEN @metadescription END WHERE nContentKey IN (@pagetitleId, @metadescId);

--Update structure table data

DECLARE @xml XML;

-- Cast from ntext to XML
SELECT @xml = CAST(cStructDescription AS XML)
FROM tblContentStructure
WHERE nStructKey = @menuid;

-- Modify the XML
SET @xml.modify('replace value of (/DisplayName/text())[1] with sql:variable("@DisplayName")');

-- Update the table with the new XML
UPDATE tblContentStructure
SET 
    cStructName = @PageName,
    cStructDescription = CAST(@xml AS NVARCHAR(MAX))
WHERE nStructKey = @menuid;



IF (@pagetitleId = 0 OR @metadescId = 0)
BEGIN

    DECLARE @newAuditId INT
	DECLARE @contentlocationAuditId INT
	DECLARE @metaContentId INT
    DECLARE @pageContentId INT
    DECLARE @metaXmlBrief NVARCHAR(MAX), @pageXmlBrief NVARCHAR(MAX)

    -- Insert MetaDescription if missing
    IF @metadescId =0 AND @metadescription IS NOT NULL AND LTRIM(RTRIM(@metadescription)) <> ''
    BEGIN
        SET @metaXmlBrief = '<Content>' + @metadescription + '</Content>'

        INSERT INTO tblAudit
        SELECT NULL, NULL, GETDATE(), 0, GETDATE(), 0, 1, NULL
        SET @newAuditId = SCOPE_IDENTITY()

		INSERT INTO tblAudit
        SELECT NULL, NULL, GETDATE(), 0, GETDATE(), 0, 1, NULL
        SET @contentlocationAuditId = SCOPE_IDENTITY()

        INSERT INTO tblContent (nContentPrimaryId, nVersion, cContentForiegnRef, cContentName, cContentSchemaName, cContentXmlBrief, cContentXmlDetail, nAuditId)
        VALUES (0, 3, '', 'MetaDescription', 'MetaData', @metaXmlBrief, '', @newAuditId)
        SET @metaContentId = SCOPE_IDENTITY()	        

        INSERT INTO tblContentLocation (nStructId, nContentId, bPrimary, bCascade, nDisplayOrder, nAuditId)
        VALUES (@menuid, @metaContentId, 1, 0, 0, @contentlocationAuditId)
    END

    -- Insert PageTitle if missing
    IF @pagetitleId =0 AND @pagetitle IS NOT NULL AND LTRIM(RTRIM(@pagetitle)) <> ''
    BEGIN
        SET @pageXmlBrief = '<Content>' + @pagetitle + '</Content>'

        INSERT INTO tblAudit
        SELECT NULL, NULL, GETDATE(), 0, GETDATE(), 0, 1, NULL
        SET @newAuditId = SCOPE_IDENTITY()

		INSERT INTO tblAudit
        SELECT NULL, NULL, GETDATE(), 0, GETDATE(), 0, 1, NULL
        SET @contentlocationAuditId = SCOPE_IDENTITY()

        INSERT INTO tblContent (nContentPrimaryId, nVersion, cContentForiegnRef, cContentName, cContentSchemaName, cContentXmlBrief, cContentXmlDetail, nAuditId)
        VALUES (0, 3, '', 'PageTitle', 'PlainText', @pageXmlBrief, '', @newAuditId)
        SET @pageContentId = SCOPE_IDENTITY()

        INSERT INTO tblContentLocation (nStructId, nContentId, bPrimary, bCascade, nDisplayOrder, nAuditId)
        VALUES (@menuid, @pageContentId, 1, 0, 0, @contentlocationAuditId)
    END
END

END