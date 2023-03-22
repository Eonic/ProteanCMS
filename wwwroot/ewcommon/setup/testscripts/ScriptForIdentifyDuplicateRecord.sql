
DECLARE @SQL AS VARCHAR(max)
DECLARE @tblNames AS TABLE (
	tableName VARCHAR(200)
	,tableKey VARCHAR(100)
	)
DECLARE @tableName AS VARCHAR(200)
DECLARE @KeyName AS VARCHAR(200)
DECLARE @CNT int=0
INSERT INTO @tblNames
SELECT c.TABLE_NAME
	,C.COLUMN_NAME
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS T
JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE C ON C.CONSTRAINT_NAME = T.CONSTRAINT_NAME
	AND T.CONSTRAINT_TYPE = 'PRIMARY KEY'
	AND c.TABLE_NAME NOT IN (
		'tblPerfMon'
		,'tblOptOutAddresses'
		,'tblSingleUsePromoCode'
		,'tblITBSupplierOfferVenues'
		,'tblActivityLog'
		,'tblEmailActivityLog'
		,'tblContentIndex'
		,'sysdiagrams'
		,'tblXmlCache'
		,'tblAudit'
		,'tblSchemaVersion'
			
		)

SET @SQL= 'SELECT A.nAuditKey,cnt.aCount FROM tblAudit A'
SET @SQL = @SQL + ' OUTER APPLY ('
SET @SQL = @SQL + ' SELECT SUM(AuditCount) As aCount FROM ('

WHILE EXISTS (
			SELECT TOP 1 tableName
			FROM @tblNames
			ORDER BY 1 ASC
			)
	BEGIN
		
		SELECT TOP 1 @tableName = tableName,@KeyName=tableKey
		FROM @tblNames
		ORDER BY tableName ASC
		if (@CNT=0)
		BEGIN
			SET @SQL = @SQL + ' SELECT COUNT(C.nAuditId) AS AuditCount FROM '+ @tableName +' C WHERE C.nAuditId = A.nAuditKey '
		END
		ELSE
		BEGIN
			SET @SQL = @SQL + ' UNION ALL SELECT COUNT(C.nAuditId) AS AuditCount FROM '+ @tableName +' C WHERE C.nAuditId = A.nAuditKey '
		END

		SET @CNT=@CNT+1
		DELETE
		FROM @tblNames
		WHERE tableName = @tableName
	END

SET @SQL = @SQL + '	) p)  CNT WHERE CNT.aCount > 2'

--PRINT(@SQL)
Exec(@SQL)
