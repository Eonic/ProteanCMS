  
CREATE PROCEDURE spGetPagesByParentPageId  
@ParentPageId as Int=null 
AS  
BEGIN  
  
 SELECT nStructKey,cStructName  
 FROM tblContentStructure cs  
 INNER JOIN tblAudit ca ON ca.nAuditKey = cs.nAuditId  
 AND ca.nStatus = 1   
 AND cs.nStructParId=isnull(@ParentPageId,0)  
 And cs.cStructForiegnRef <>''  
 order by nStructOrder  
END  