CREATE PROCEDURE dbo.spGetDirectoryItems

	(
		@cSchemaName nvarchar(50),
		@nParDirId integer = 0
	)
AS
	IF @nParDirId = 0
	BEGIN
		SELECT
		d.nDirKey as id, 
		dbo.fxn_getStatus(a.nAuditKey,getdate()) as Status,  
		d.cDirName as [Name], 
		d.cDirXml as Details  
		FROM tblDirectory d
		INNER JOIN tblAudit a on nAuditId = a.nAuditKey   
		WHERE cDirSchema = @cSchemaName
		order by d.cDirName 
	END
	ELSE
	BEGIN
		SELECT
		d.nDirKey as id, 
		dbo.fxn_getStatus(a.nAuditKey,getdate()) as Status,  
		d.cDirName as [Name], 
		d.cDirXml as Details
		
		FROM tblDirectory d
		INNER JOIN tblAudit a on nAuditId = a.nAuditKey 
		INNER JOIN tblDirectoryRelation dr on nDirKey = dr.nDirChildId 
	    
		WHERE cDirSchema = @cSchemaName 
		AND dr.nDirParentId =  @nParDirId
		order by d.cDirName 
	END
	RETURN 
