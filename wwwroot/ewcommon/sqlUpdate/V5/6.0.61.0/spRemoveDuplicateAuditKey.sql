

/****** Object:  StoredProcedure [dbo].[spRemoveDuplicateAuditKey]    Script Date: 21-03-2023 12:57:06 ******/
DROP PROCEDURE [dbo].[spRemoveDuplicateAuditKey]
GO

/****** Object:  StoredProcedure [dbo].[spRemoveDuplicateAuditKey]    Script Date: 21-03-2023 12:57:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[spRemoveDuplicateAuditKey] @Key VARCHAR(100) 
	,@TableName VARCHAR(200)  
AS
BEGIN
	DECLARE @SQL AS VARCHAR(max)
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
			,@TableName
			)

	SET @SQL = 'CREATE TABLE #DuplicateAuditIdTable (nId bigint, nAuditId bigint); '
	SET @SQL = @SQL + ' INSERT INTO #DuplicateAuditIdTable (nId, nAuditId) '
	SET @SQL = @SQL + 'SELECT ' + @Key + ' , nAuditId FROM ' + @TableName + ' c WHERE (SELECT COUNT(*) FROM	' + @TableName + ' c1 WHERE c.nAuditId=c1.nAuditId) >1;'
	SET @SQL = @SQL + ' CREATE TABLE #NewAuditIDTable (nId bigint, nAuditId bigint);'
	SET @SQL = @SQL + ' INSERT INTO #NewAuditIDTable (nId, nAuditId)'
	SET @SQL = @SQL + ' SELECT max(nId),t.nAuditId FROM #DuplicateAuditIdTable t INNER JOIN ' + @TableName + ' c on t.nAuditid=c.nAuditId group by t.nAuditid;'

	WHILE EXISTS (
			SELECT TOP 1 tableName
			FROM @tblNames
			ORDER BY 1 ASC
			)
	BEGIN
		SELECT TOP 1 @name = tableName,@KeyName=tableKey
		FROM @tblNames
		ORDER BY tableName ASC

		SET @SQL = @SQL + ' INSERT INTO #NewAuditIDTable (nId, nAuditId)'
		SET @SQL = @SQL + ' SELECT '+ @Key +',nAuditId FROM '+ @TableName +' WHERE nAuditId IN (SELECT nAuditId FROM '+@name+')
'

		DELETE
		FROM @tblNames
		WHERE tableName = @name
	END

	SET @SQL = @SQL + 'DECLARE @nAuditId bigint '
	SET @SQL = @SQL + 'DECLARE @nNewAuditId bigint '
	SET @SQL = @SQL + 'DECLARE @nId bigint '
	SET @SQL = @SQL + ' WHILE exists(SELECT TOP 1 nId FROM #NewAuditIDTable ORDER BY nId)'
	SET @SQL = @SQL + ' BEGIN'
	SET @SQL = @SQL + ' SELECT TOP 1 @nId=nId, @nAuditId=nAuditId FROM #NewAuditIDTable ORDER BY nId'
	SET @SQL = @SQL + ' IF exists(select @nAuditId) AND @nAuditId!=0'
	SET @SQL = @SQL + ' BEGIN '
	SET @SQL = @SQL + ' INSERT INTO [dbo].[tblAudit]([dPublishDate],[dExpireDate],[dINSERTDate],[nINSERTDirId],[dUpdateDate],[nUpdateDirId],[nStatus],[cDescription])'
	SET @SQL = @SQL + ' SELECT  dPublishDate, dExpireDate,dINSERTDate,isnull(nINSERTDirId,1),dUpdateDate, isnull(nUpdateDirId,1),nStatus,cDescription FROM tblAudit WHERE nAuditKey=@nAuditId'	
	SET @SQL = @SQL + ' SELECT @nNewAuditId=SCOPE_IDENTITY()'
	SET @SQL = @SQL + ' END ELSE'
	SET @SQL = @SQL + ' BEGIN '
	SET @SQL = @SQL + ' INSERT INTO [dbo].[tblAudit]([dPublishDate],[dExpireDate],[dINSERTDate],[nINSERTDirId],[dUpdateDate],[nUpdateDirId],[nStatus],[cDescription])'
	SET @SQL = @SQL + ' SELECT  getDate(),  getDate(), getDate(),1, getDate(), 1,0,''Created Link record where audit id doesnt exists'''	
	SET @SQL = @SQL + ' SELECT @nNewAuditId=SCOPE_IDENTITY()'
	SET @SQL = @SQL + ' END '
	SET @SQL = @SQL + ' UPDATE '+ @TableName+ ' SET nAuditId=@nNewAuditId WHERE '+@Key+' = @nId'
	SET @SQL = @SQL + ' DELETE FROM #NewAuditIDTable WHERE nId=@nId AND nAuditId=@nAuditId'
	SET @SQL = @SQL + ' END'
	SET @SQL = @SQL + ' DROP TABLE #NewAuditIDTable'
	SET @SQL = @SQL + ' DROP TABLE #DuplicateAuditIdTable'

	--PRINT (@SQL)

	EXEC (@SQL)
END
GO


