
CREATE PROCEDURE spGetPagesByParentPageId       
@PageId as Int=null   ,  
@whereSql varchar(max)     
AS            
BEGIN      
  Declare @sqlQuery as nvarchar(max)  =''    
  Declare @sqlQuery2 as nvarchar(max)  =''    
    Declare @sqlFinalQuery as nvarchar(max)  =''    
  
  
 If( isnull(@PageId,1)=1)       
 BEGIN      
   set @sqlQuery =' SELECT nStructKey,cStructName,ProductCount, cStructName +'' ['' + convert(varchar(5),ProductCount)+'']'' as [name] from      
 (      
 SELECT nStructKey,Convert(XML,cStructDescription).value(''(/DisplayName/node())[1]'', ''nvarchar(max)'') as cStructName ,cs.nStructOrder,       
  (select count(*) from tblContent c  inner join tblContentLocation cl on cl.nContentId=c.nContentKey inner join tblAudit a on a.nAuditKey=c.nAuditId and a.nStatus=1      
  inner join tblaudit cla on cla.nAuditKey=cl.nAuditId and cla.nStatus=1      
  inner join tblContentStructure cs1 on cs1.nStructKey=cl.nStructId inner join tblAudit csa1 on csa1.nAuditKey=cs1.nAuditId and csa1.nStatus=1      
  inner join tblContentStructure cs2 on cs2.nStructKey= cs1.nStructKey inner join tblAudit csa2 on csa2.nAuditKey=cs2.nAuditId and csa2.nStatus=1      
  and cs2.nStructParId=cs.nStructKey and c.cContentSchemaName=''Product'''  
   
 set @sqlQuery2= ' ) as ProductCount      
 FROM tblContentStructure cs        
  INNER JOIN tblAudit ca ON ca.nAuditKey = cs.nAuditId            
  AND ca.nStatus = 1             
  AND cs.nStructParId= 1         
  And cs.cStructForiegnRef <>''''            
        
  ) a where ProductCount<>0 order by nStructOrder '     
        
 if (@whereSql<>'')       
        
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
  
    exec (@sqlFinalQuery)  
 END      
 ELSE      
 BEGIN    
  set @sqlQuery =' SELECT nStructKey,cStructName,ProductCount, cStructName +'' ['' + convert(varchar(5),ProductCount)+'']'' as [name] from     
 (      
 SELECT nStructKey,Convert(XML,cStructDescription).value(''(/DisplayName/node())[1]'', ''nvarchar(max)'') as cStructName , cs.nStructOrder,      
 (select count(*) from tblContent c  inner join tblContentLocation cl on cl.nContentId=c.nContentKey  inner join tblAudit a on a.nAuditKey=c.nAuditId and a.nStatus=1      
 and cl.nStructId=nStructKey inner join tblaudit cla on cla.nAuditKey=cl.nAuditId and cla.nStatus=1 and c.cContentSchemaName=''Product'''  
  
 set @sqlQuery2=') as ProductCount      
 FROM tblContentStructure cs            
 INNER JOIN tblAudit ca ON ca.nAuditKey = cs.nAuditId            
 AND ca.nStatus = 1             
 AND cs.nStructParId= isnull(64,1)            
 And cs.cStructForiegnRef <>''''   
 ) a where ProductCount<>0 order by nStructOrder  '  
  if (@whereSql<>'')       
        
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
  
    exec (@sqlFinalQuery)  
 END   
END 