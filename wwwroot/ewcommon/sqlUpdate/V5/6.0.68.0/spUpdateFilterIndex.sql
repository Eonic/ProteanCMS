-- spUpdateFilterIndex  'SKU',3        
CREATE PROCEDURE [dbo].[spUpdateFilterIndex] @SchemaName VARCHAR(50)
	,@IndexId INT
AS
BEGIN
	DECLARE @isParentRefForSKU AS INT = (
			SELECT bProductRefForSKU
			FROM tblContentIndexDef
			WHERE nContentIndexDefKey = @IndexId
			)
	DECLARE @defName AS VARCHAR(200) = (
			SELECT cDefinitionName
			FROM tblContentIndexDef
			WHERE nContentIndexDefKey = @IndexId
			)
	DECLARE @fieldPath AS VARCHAR(max) = (
			SELECT cContentValueXpath
			FROM tblContentIndexDef
			WHERE nContentIndexDefKey = @IndexId
			)
	DECLARE @cContentDataType AS VARCHAR(max) = (
			SELECT nContentIndexDataType
			FROM tblContentIndexDef
			WHERE nContentIndexDefKey = @IndexId
			)
	--DECLARE @cContentSchemaName AS varchar(max)=(select cContentSchemaName from tblContentIndexDef where nContentIndexDefKey= @IndexId)        
	DECLARE @dataType AS VARCHAR(max)
	DECLARE @sql AS VARCHAR(max)

	IF (@cContentDataType = 1) --number        
	BEGIN
		SET @dataType = 'float'
	END
	ELSE IF (
			@cContentDataType = 2
			OR @cContentDataType = 3
			) --text or date        
	BEGIN
		SET @dataType = 'varchar(20)'
	END

	IF (@isParentRefForSKU = 1)
	BEGIN
		DECLARE @tblSkuAndValue AS TABLE (
			SKUId INT
			,nValue INT
			)
		DECLARE @query AS VARCHAR(max) = 'select nContentKey, CONVERT(XML, cContentXmlBrief).value(''' + @fieldPath + ''',''' + @dataType + ''') as price from tblcontent tc        
join tblAudit a on tc.nAuditId=a.nAuditKey        
where cContentSchemaName=''SKU'' and a.nStatus=1'

		INSERT INTO @tblSkuAndValue
		EXEC (@query)

		--  select * from @tblSkuAndValue        
		DECLARE @tblRelation AS TABLE (
			parentId INT
			,childId INT
			)
		DECLARE @strQuery AS VARCHAR(max) = 'select   cr.nContentParentId ,cr.nContentChildId from tblContentRelation cr        
join tblContent c on cr.nContentParentId=c.nContentKey        
where c.cContentSchemaName=''Product'' and cr.nContentChildId in(        
select nContentKey  from tblcontent tc        
join tblAudit a on tc.nAuditId=a.nAuditKey        
where cContentSchemaName=''SKU'' and a.nStatus=1)'

		INSERT INTO @tblRelation
		EXEC (@strQuery)

		-- select * from @tblRelation        
		BEGIN TRAN filterIndex

		DELETE
		FROM tblContentIndex
		WHERE nContentIndexDefinitionKey = @IndexId

		INSERT INTO tblContentIndex (
			[nContentId]
			,[nContentIndexDefinitionKey]
			,[nNumberValue]
			)
		SELECT tr.parentId
			,@IndexId
			,sp.nValue
		FROM @tblRelation tr
		INNER JOIN @tblSkuAndValue sp ON tr.childId = sp.SKUId

		COMMIT TRAN filterIndex
	END
	ELSE
	BEGIN
		SET @sql = '  Insert into [tblContentIndex]([nContentId],[nContentIndexDefinitionKey],[nNumberValue])                
    select c.nContentKey,' + convert(VARCHAR(100), @IndexId) + ', IsNULL(convert(xml,c.cContentXmlDetail).value(''' + @fieldPath + ''',''' + @dataType + '''),0)          
  from tblContent c where c.cContentSchemaName=''' + @SchemaName + ''''

		DELETE
		FROM tblContentIndex
		WHERE nContentIndexDefinitionKey = @IndexId

		EXEC (@sql)
	END
END