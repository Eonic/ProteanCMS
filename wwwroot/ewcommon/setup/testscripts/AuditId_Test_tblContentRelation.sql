--Tests
---------------------------------------------------------------------------------------
	SELECT 'No AuditId'

	--No AuditId
	SELECT nContentRelationKey, nAuditId
	FROM tblContentRelation c 
	WHERE nAuditId IS NULL OR nAuditId = 0

	SELECT 'Invalid AuditIds'

	--Invalid AuditId's. Does not exist in the tblAudit table.
	SELECT nContentRelationKey, nAuditId
	FROM tblContentRelation C
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
			ROW_NUMBER() OVER (PARTITION BY nAuditId ORDER BY nContentRelationKey) Row_Num
		FROM tblContentRelation C
		WHERE nAuditId >  0
	)
	
	SELECT nContentRelationKey, nAuditId
	FROM TEMP T
	JOIN tblAudit A ON A.nAuditKey = T.nAuditId --Duplicate should not consider not existent audit ids
	WHERE Row_Num > 1