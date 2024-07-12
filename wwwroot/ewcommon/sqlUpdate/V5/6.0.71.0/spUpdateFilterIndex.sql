
/****** Object:  StoredProcedure [dbo].[spUpdateFilterIndex]    Script Date: 28-05-2024 15:50:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- spUpdateFilterIndex  'PRODUCT',4 
ALTER PROCEDURE [dbo].[spUpdateFilterIndex] @SchemaName VARCHAR(50)
	,@IndexId INT
AS
BEGIN
	DECLARE @isParentRefForSKU AS INT 
	DECLARE @defName AS VARCHAR(200)
	DECLARE @fieldPath AS VARCHAR(max)
	DECLARE @cContentDataType AS VARCHAR(max)
	DECLARE @cDefaultValue As VARCHAR(20)
			SELECT @isParentRefForSKU=bProductRefForSKU,@defName=cDefinitionName,
			@fieldPath=cContentValueXpath,@cContentDataType=nContentIndexDataType,
			@cDefaultValue=isnull(cDefaultValue,'')
			FROM tblContentIndexDef
			WHERE nContentIndexDefKey = @IndexId
			      
	DECLARE @dataType AS VARCHAR(max)
	DECLARE @sql AS VARCHAR(max)

	IF (@cContentDataType = 1) --number        
	BEGIN
		SET @dataType = 'float'
	END
	ELSE IF (@cContentDataType = 2) --text 
	BEGIN
		SET @dataType = 'varchar(20)'
	END
	ELSE IF (@cContentDataType = 3) -- date        
	BEGIN
		SET @dataType = 'datetime'
	END

	IF (@isParentRefForSKU = 1)
	BEGIN
		DECLARE @tblSkuAndValue AS TABLE (
			SKUId INT
			,nValue VARCHAR(20)
			)
		DECLARE @query AS VARCHAR(max)

		IF (@cContentDataType = 3)
		BEGIN
			SET @query = 'select nContentKey,isnull( CONVERT(XML, cContentXmlBrief).value(''' + @fieldPath + ''',''' + @dataType + '''),''1900-01-01'') as price from tblcontent tc        
join tblAudit a on tc.nAuditId=a.nAuditKey        
where cContentSchemaName=''SKU'' and a.nStatus=1'
		END
		ELSE
		BEGIN
			SET @query = 'select nContentKey, CONVERT(XML, cContentXmlBrief).value(''' + @fieldPath + ''',''' + @dataType + ''') as price from tblcontent tc        
join tblAudit a on tc.nAuditId=a.nAuditKey        
where cContentSchemaName=''SKU'' and a.nStatus=1'
		END

		INSERT INTO @tblSkuAndValue
		EXEC (@query)

		-- select * from @tblSkuAndValue        
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

		IF (@cContentDataType = 1)
		BEGIN
			INSERT INTO tblContentIndex (
				[nContentId]
				,[nContentIndexDefinitionKey]
				,[nNumberValue]
				)
			SELECT tr.parentId
				,@IndexId
				,CASE when (isnull(Convert(float,sp.nValue),0)=0 or Convert(float,sp.nValue)=0) then Convert(float,@cDefaultValue) else Convert(float,sp.nValue) end
			FROM @tblRelation tr
			INNER JOIN @tblSkuAndValue sp ON tr.childId = sp.SKUId
		END
		ELSE IF (@cContentDataType = 2) --text         
		BEGIN
			INSERT INTO tblContentIndex (
				[nContentId]
				,[nContentIndexDefinitionKey]
				,[cTextValue]
				)
			SELECT tr.parentId
				,@IndexId
				,CASE when isnull(sp.nValue,'')='' then @cDefaultValue else sp.nValue end
			FROM @tblRelation tr
			INNER JOIN @tblSkuAndValue sp ON tr.childId = sp.SKUId
		END
		ELSE IF (@cContentDataType = 3) --Date         
		BEGIN
			INSERT INTO tblContentIndex (
				[nContentId]
				,[nContentIndexDefinitionKey]
				,[dDateValue]
				)
			SELECT tr.parentId
				,@IndexId
				,CASE when isnull(sp.nValue,'')='' then @cDefaultValue else sp.nValue end
			FROM @tblRelation tr
			INNER JOIN @tblSkuAndValue sp ON tr.childId = sp.SKUId
		END

		COMMIT TRAN filterIndex
	END
	ELSE
	BEGIN
	
		--SET @sql = '  Insert into [tblContentIndex]([nContentId],[nContentIndexDefinitionKey],[nNumberValue])                
  --  select c.nContentKey,' + convert(VARCHAR(100), @IndexId) + ', IsNULL(convert(xml,c.cContentXmlDetail).value(''' + @fieldPath + ''',''' + @dataType + '''),'''+@cDefaultValue+''')          
  --from tblContent c where c.cContentSchemaName=''' + @SchemaName + ''''

  SET @sql = '  Insert into [tblContentIndex]([nContentId],[nContentIndexDefinitionKey],[nNumberValue])                
    select c.nContentKey,'+ convert(VARCHAR(100), @IndexId) +  
  +', CASE when (isnull(convert(xml,c.cContentXmlDetail).value( '''+ @fieldPath + ''',''' + @dataType + '''),'''')='''') then '+ convert(varchar(20),@cDefaultValue)+' else convert(xml,c.cContentXmlDetail).value(''' + @fieldPath + ''',''' + @dataType + ''') end  from tblContent c where c.cContentSchemaName=''' + @SchemaName + ''''

		DELETE
		FROM tblContentIndex
		WHERE nContentIndexDefinitionKey = @IndexId

		EXEC (@sql)
	END
END