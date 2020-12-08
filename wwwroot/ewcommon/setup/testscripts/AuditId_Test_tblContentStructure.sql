--Tests
---------------------------------------------------------------------------------------
	SELECT 'No AuditId'

	--No AuditId
	SELECT nStructKey, nAuditId
	FROM tblContentStructure c 
	WHERE nAuditId IS NULL OR nAuditId = 0

	SELECT 'Invalid AuditIds'

	--Invalid AuditId's. Does not exist in the tblAudit table.
	SELECT nStructKey, nAuditId
	FROM tblContentStructure C
	WHERE NOT EXISTS
	(
		SELECT 1
		FROM tblAudit A
		WHERE A.nAuditKey = C.nAuditId
	)

	SELECT 'Duplicate AuditIds'

	--Duplicate AuditIds
	;WITH TEMP AS
	(
		SELECT
			*,
			ROW_NUMBER() OVER (PARTITION BY nAuditId ORDER BY nStructKey) Row_Num
		FROM tblContentStructure C
		WHERE nAuditId >  0
	)
	
	SELECT nStructKey, nAuditId
	FROM TEMP
	WHERE Row_Num > 1