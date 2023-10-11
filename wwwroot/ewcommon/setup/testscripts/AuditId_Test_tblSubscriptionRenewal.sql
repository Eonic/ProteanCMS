--Tests
---------------------------------------------------------------------------------------
	SELECT 'No AuditId'

	--No AuditId
	SELECT nSubRenewalKey, nAuditId
	FROM tblSubscriptionRenewal D 
	WHERE nAuditId IS NULL OR nAuditId = 0

	SELECT 'Invalid AuditIds'

	--Invalid AuditId's. Does not exist in the tblAudit table.
	SELECT nSubRenewalKey, nAuditId
	FROM tblSubscriptionRenewal D
	WHERE NOT EXISTS
	(
		SELECT 1
		FROM tblAudit A
		WHERE A.nAuditKey = D.nAuditId
	)

	SELECT 'Duplicate AuditIds'

	--Duplicate AuditIds
	;WITH TEMP AS
	(
		SELECT
			*,
			ROW_NUMBER() OVER (PARTITION BY nAuditId ORDER BY nSubRenewalKey) Row_Num
		FROM tblSubscriptionRenewal D
		WHERE nAuditId >  0
	)
	
	SELECT nSubRenewalKey, nAuditId
	FROM TEMP T
	JOIN tblAudit A ON A.nAuditKey = T.nAuditId --Duplicate should not consider not existent audit ids
	WHERE Row_Num > 1