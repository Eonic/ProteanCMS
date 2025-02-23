
CREATE PROCEDURE [dbo].[spScheduleToUpdateIndexTable]
@ContentSchemaName varchar(50)
AS
BEGIN

DECLARE @ContentTable AS TABLE(ContentKey int, SchemaName varchar(10))
DECLARE @nContentKey As int
DECLARE @cContentSchemaName varchar(15)
INSERT INTO @ContentTable SELECT nContentKey,cContentSchemaName from tblContent where cContentSchemaName=@ContentSchemaName

WHILE EXISTS (  
   SELECT *  
   FROM @ContentTable  
   )  
   BEGIN

	   SELECT TOP 1 @nContentKey=ContentKey,@cContentSchemaName=SchemaName from @ContentTable;

	   EXEC spUpdateContentIndexData @cContentSchemaName,@nContentKey

	  DELETE  
	  FROM @ContentTable  
	  WHERE ContentKey = @nContentKey  
  
	END  
 
END