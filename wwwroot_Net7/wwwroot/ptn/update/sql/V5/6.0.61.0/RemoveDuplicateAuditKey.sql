DECLARE @tblNames AS TABLE (
		tableName VARCHAR(200)
		,tableKey VARCHAR(100)
		)
DECLARE @Name AS VARCHAR(200)
DECLARE @KeyName AS VARCHAR(200)

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


WHILE EXISTS (
			SELECT TOP 1 tableName
			FROM @tblNames
			ORDER BY 1 ASC
			)
	BEGIN
		SELECT TOP 1 @name = tableName,@KeyName=tableKey
		FROM @tblNames
		ORDER BY tableName ASC

		 exec spRemoveDuplicateAuditKey @KeyName,@name

		DELETE
		FROM @tblNames
		WHERE tableName = @name
	END

