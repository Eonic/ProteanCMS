IF OBJECT_ID('spGetPagesByParentPageId', 'P') IS NOT NULL
DROP PROC spGetPagesByParentPageId
GO

-- [spGetPagesByParentPageId] 64,'  nContentId in (select nContentId from tblCartCatProductRelations c inner join tblAudit a on a.nAuditKey=c.nAuditId and nStatus=1 where c.nCatId in (9)) ','Product'            
            
CREATE PROCEDURE [dbo].[spGetPagesByParentPageId]                
@PageId as Int=null   ,            
@whereSql varchar(max) ,          
@FilterTarget nvarchar(10)          
AS                      
BEGIN                
  Declare @sqlQuery as nvarchar(max)  =''              
  Declare @sqlQuery2 as nvarchar(max)  =''              
    Declare @sqlFinalQuery as nvarchar(max)  =''              
            
            
 If( isnull(@PageId,1)=1)                 
 BEGIN                
   set @sqlQuery =' SELECT nStructKey,cStructName,ContentCount from                
 (                
 SELECT nStructKey,Convert(XML,cStructDescription).value(''(/DisplayName/node())[1]'', ''nvarchar(max)'') as cStructName ,cs.nStructOrder,                 
  (select count(*) from tblContent c  inner join tblContentLocation cl on cl.nContentId=c.nContentKey inner join tblAudit a on a.nAuditKey=c.nAuditId and a.nStatus=1                
  inner join tblaudit cla on cla.nAuditKey=cl.nAuditId and cla.nStatus=1                
  inner join tblContentStructure cs1 on cs1.nStructKey=cl.nStructId inner join tblAudit csa1 on csa1.nAuditKey=cs1.nAuditId and csa1.nStatus=1                
  inner join tblContentStructure cs2 on cs2.nStructKey= cs1.nStructKey inner join tblAudit csa2 on csa2.nAuditKey=cs2.nAuditId and csa2.nStatus=1                
  and cs2.nStructParId=cs.nStructKey and c.cContentSchemaName='''+@FilterTarget+''''           
             
 set @sqlQuery2= ' ) as ContentCount                
 FROM tblContentStructure cs                  
  INNER JOIN tblAudit ca ON ca.nAuditKey = cs.nAuditId                      
  AND ca.nStatus = 1                       
  AND cs.nStructParId= 1                   
  And cs.cStructForiegnRef <>''''                      
                  
  ) a where ContentCount<>0 order by nStructOrder '               
                  
 if (@whereSql<>'')         
 BEGIN        
 SET @whereSql=' AND ' +@whereSql        
 END        
                  
set @sqlFinalQuery= CONCAT (                              
   @sqlQuery                              
   ,@whereSql                              
   )                    
  set @sqlFinalQuery= CONCAT (                              
   @sqlFinalQuery                              
   ,@sqlQuery2                          
   )                
   if (@whereSql='')                
              
    set @sqlFinalQuery= CONCAT (                              
   @sqlQuery                              
   ,@sqlQuery2                              
   )               
     --  print (@sqlFinalQuery)            
exec (@sqlFinalQuery)            
 END                
 ELSE                
 BEGIN              
  set @sqlQuery =' SELECT nStructKey,cStructName,ContentCount from               
 (                
 SELECT nStructKey,Convert(XML,cStructDescription).value(''(/DisplayName/node())[1]'', ''nvarchar(max)'') as cStructName , cs.nStructOrder,                
 (select count(*) from tblContent c  inner join tblContentLocation cl on cl.nContentId=c.nContentKey  inner join tblAudit a on a.nAuditKey=c.nAuditId and a.nStatus=1                
 and cl.nStructId=nStructKey inner join tblaudit cla on cla.nAuditKey=cl.nAuditId and cla.nStatus=1 and c.cContentSchemaName='''+@FilterTarget+''''             
            
 set @sqlQuery2=') as ContentCount                
 FROM tblContentStructure cs                      
 INNER JOIN tblAudit ca ON ca.nAuditKey = cs.nAuditId                      
 AND ca.nStatus = 1                       
 AND cs.nStructParId= isnull('+convert(varchar(10),@PageId)+',1)                      
 And cs.cStructForiegnRef <>''''             
 ) a where ContentCount<>0 order by nStructOrder  '            
  if (@whereSql<>'')         
 BEGIN        
 SET @whereSql=' AND ' + @whereSql        
 END            
      
set @sqlFinalQuery= CONCAT (                              
   @sqlQuery                              
   ,@whereSql                              
   )                  
  set @sqlFinalQuery= CONCAT (                              
   @sqlFinalQuery                              
   ,@sqlQuery2                          
   )                
   if (@whereSql='')                
              
    set @sqlFinalQuery= CONCAT (                              
   @sqlQuery                              
   ,@sqlQuery2                              
   )               
    -- print(@sqlFinalQuery)      
   exec (@sqlFinalQuery)            
 END             
END 