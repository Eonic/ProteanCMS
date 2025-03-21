CREATE PROCEDURE [dbo].[spUpdateContentIndexData] 
@cContentSchemaName varchar(50),  
@contentKey varchar(10)  
AS  
BEGIN  
DECLARE @sql AS VARCHAR(max)  
 DECLARE @fieldPath AS VARCHAR(200)  
 DECLARE @dataType AS INT  
 DECLARE @ContentIndexDefid AS INT  
 DECLARE @temp AS TABLE (  
  id INT  
  ,xpath VARCHAR(500)  
  ,datatype INT  
  ,ContentSchemaName varchar(50)  
  )  
  
 INSERT INTO @temp  
 SELECT cd.nContentIndexDefKey  
  ,cd.cContentValueXpath  
  ,cd.nContentIndexDataType  
  ,cd.cContentSchemaName  
 FROM tblContentIndexDef cd  
 WHERE cd.cContentSchemaName = @cContentSchemaName  
  
 WHILE EXISTS (  
   SELECT *  
   FROM @temp  
   )  
 BEGIN  
  SELECT @ContentIndexDefid = id  
   ,@fieldPath = xpath  
   ,@dataType = datatype  
  FROM @temp  
  
  IF (@dataType = 1) --number  
  BEGIN  
   IF EXISTS (  
     SELECT *  
     FROM tblContentIndex  
     WHERE nContentId = Convert(int,@contentKey)  
      AND nContentIndexDefinitionKey = @ContentIndexDefid  
     )  
   BEGIN  
    SET @sql = 'Update ci set nNumberValue=convert(xml,c.cContentXmlDetail).value(''' + @fieldPath + ''',''int'')  
       from tblContentIndex ci inner join tblContent c on  ci.nContentId=c.ncontentKey and ci.nContentIndexDefinitionKey='  
       + convert(VARCHAR(100), @ContentIndexDefid) + ' and c.nContentKey=' + @contentKey  
         
   END  
   ELSE  
   BEGIN  
    SET @sql = 'Insert into [tblContentIndex]([nContentId],[nContentIndexDefinitionKey],[nNumberValue])  
       select c.nContentKey,' + convert(VARCHAR(100), @ContentIndexDefid) + ',convert(xml,c.cContentXmlDetail).value(''' + @fieldPath + ''',''int'')  
       from tblContent c where c.nContentKey=' + @contentKey  
   END  
  END  
  
  IF (@dataType = 2) -- text  
  BEGIN  
   IF EXISTS (  
     SELECT *  
     FROM tblContentIndex  
     WHERE nContentId = @contentKey  
      AND nContentIndexDefinitionKey = @ContentIndexDefid  
     )  
   BEGIN  
    SET @sql = 'Update ci set [cTextValue]=convert(xml,c.cContentXmlDetail).value(''' + @fieldPath + ''',''varchar(20)'')  
       from tblContentIndex ci inner join tblContent c on  ci.nContentId=c.ncontentKey and ci.nContentIndexDefinitionKey=' + convert(VARCHAR(100), @ContentIndexDefid) + ' and c.nContentKey=' + @contentKey  
   END  
   ELSE  
   BEGIN  
    SET @sql = '  
      Insert into [tblContentIndex]([nContentId],[nContentIndexDefinitionKey],[cTextValue])  
      select c.nContentKey,' + convert(VARCHAR(100), @ContentIndexDefid) + ',convert(xml,c.cContentXmlDetail).value(''' + @fieldPath + ''',''varchar(20)'')  
      from tblContent c where c.nContentKey=' + @contentKey  
   END  
  END  
 
  IF (@dataType = 3) -- date  
  BEGIN  
   IF EXISTS (  
     SELECT *  
     FROM tblContentIndex  
     WHERE nContentId = @contentKey  
      AND nContentIndexDefinitionKey = @ContentIndexDefid  
     )  
   BEGIN  
    SET @sql = 'Update ci set [dDateValue]=convert(xml,c.cContentXmlDetail).value(''' + @fieldPath + ''',''varchar(20)'')  
       from tblContentIndex ci inner join tblContent c on  ci.nContentId=c.ncontentKey and ci.nContentIndexDefinitionKey=' + convert(VARCHAR(100), @ContentIndexDefid) + ' and c.nContentKey=' + @contentKey  
   END  
   ELSE  
   BEGIN  
    SET @sql = '  
      Insert into [tblContentIndex]([nContentId],[nContentIndexDefinitionKey],[dDateValue])  
      select c.nContentKey,' + convert(VARCHAR(100), @ContentIndexDefid) + ',convert(xml,c.cContentXmlDetail).value(''' + @fieldPath + ''',''varchar(20)'')  
      from tblContent c where c.nContentKey=' + @contentKey  
   END  
  END  
  EXEC (@sql)  
  
  DELETE  
  FROM @temp  
  WHERE id = @ContentIndexDefid  
  
END  
END