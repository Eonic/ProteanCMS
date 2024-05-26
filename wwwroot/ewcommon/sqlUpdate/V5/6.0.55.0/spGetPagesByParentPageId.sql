        
CREATE PROCEDURE dbo.spGetPagesByParentPageId   
@PageId as Int=null       
AS        
BEGIN        
 If( isnull(@PageId,1)=1)   
 BEGIN  
    
 SELECT nStructKey,cStructName,ProductCount, cStructName +' [' + convert(varchar(5),ProductCount)+']' as [name] from  
 (  
 SELECT nStructKey,Convert(XML,cStructDescription).value('(/DisplayName/node())[1]', 'nvarchar(max)') as cStructName ,cs.nStructOrder,   
  (select count(*) from tblContent c  inner join tblContentLocation cl on cl.nContentId=c.nContentKey inner join tblAudit a on a.nAuditKey=c.nAuditId and a.nStatus=1  
  inner join tblaudit cla on cla.nAuditKey=cl.nAuditId and cla.nStatus=1  
  inner join tblContentStructure cs1 on cs1.nStructKey=cl.nStructId inner join tblAudit csa1 on csa1.nAuditKey=cs1.nAuditId and csa1.nStatus=1  
  inner join tblContentStructure cs2 on cs2.nStructKey= cs1.nStructKey inner join tblAudit csa2 on csa2.nAuditKey=cs2.nAuditId and csa2.nStatus=1  
  and cs2.nStructParId=cs.nStructKey and c.cContentSchemaName='Product') as ProductCount  
 FROM tblContentStructure cs    
  INNER JOIN tblAudit ca ON ca.nAuditKey = cs.nAuditId        
  AND ca.nStatus = 1         
  AND cs.nStructParId= 1     
  And cs.cStructForiegnRef <>''        
    
  ) a where ProductCount<>0 order by nStructOrder  
    
 END  
 ELSE  
 BEGIN  
 SELECT nStructKey,cStructName,ProductCount, cStructName +' [' + convert(varchar(5),ProductCount)+']' as [name]  from  
 (  
 SELECT nStructKey,Convert(XML,cStructDescription).value('(/DisplayName/node())[1]', 'nvarchar(max)') as cStructName , cs.nStructOrder,  
 (select count(*) from tblContent c  inner join tblContentLocation cl on cl.nContentId=c.nContentKey  inner join tblAudit a on a.nAuditKey=c.nAuditId and a.nStatus=1  
 and cl.nStructId=nStructKey inner join tblaudit cla on cla.nAuditKey=cl.nAuditId and cla.nStatus=1 and c.cContentSchemaName='Product') as ProductCount  
 FROM tblContentStructure cs        
 INNER JOIN tblAudit ca ON ca.nAuditKey = cs.nAuditId        
 AND ca.nStatus = 1         
 AND cs.nStructParId= isnull(@PageId,1)        
 And cs.cStructForiegnRef <>''  ) a where ProductCount<>0 order by nStructOrder  
 END  
         
END 