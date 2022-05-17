--Tests
---------------------------------------------------------------------------------------
	SELECT 'No AuditId'

	--No AuditId
	SELECT nCartItemKey, nAuditId
	FROM tblCartItem C 
	WHERE nAuditId IS NULL OR nAuditId = 0

	SELECT 'Invalid AuditIds'

	--Invalid AuditId's. Does not exist in the tblAudit table.
	SELECT nCartItemKey, nAuditId
	FROM tblCartItem C 
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
			ROW_NUMBER() OVER (PARTITION BY nAuditId ORDER BY nCartItemKey) Row_Num
		FROM tblCartItem C
		WHERE nAuditId >  0
	)
	
	SELECT nCartItemKey, nAuditId
	FROM TEMP T
	JOIN tblAudit A ON A.nAuditKey = T.nAuditId --Duplicate should not consider not existent audit ids
	WHERE Row_Num > 1