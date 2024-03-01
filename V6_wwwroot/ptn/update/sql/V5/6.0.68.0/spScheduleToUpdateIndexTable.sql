Create PROCEDURE [spScheduleToUpdateIndexTable] --'' -- 2     
@IndexId int        
AS        
BEGIN  
if(@IndexId!='')
BEGIN
DECLARE @cContentSchemaName AS varchar(max)=(select cContentSchemaName from tblContentIndexDef where nContentIndexDefKey= @IndexId)  
exec  spUpdateFilterIndex @cContentSchemaName ,@IndexId
 end else
 BEGIN
Declare @nContentIndexDefKey   int 
Declare @SchemaName varchar(20)

Declare filterIndex CURSOR FOR    
 SELECT cd.nContentIndexDefKey 
  ,cd.cContentSchemaName            
 FROM tblContentIndexDef cd  
Open filterIndex    
 Fetch next from filterIndex into @nContentIndexDefKey ,  @SchemaName
while(@@FETCH_STATUS=0)  
BEGIN         
print @SchemaName
print @nContentIndexDefKey
 EXEC spUpdateFilterIndex @SchemaName , @nContentIndexDefKey
 Fetch next from filterIndex into @nContentIndexDefKey ,  @SchemaName

end
  CLOSE filterIndex;  
DEALLOCATE filterIndex;  
 end

 end