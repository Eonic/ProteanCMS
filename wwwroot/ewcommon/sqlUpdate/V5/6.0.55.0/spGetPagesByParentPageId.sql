    
CREATE PROCEDURE spGetPagesByParentPageId 
@PageId as Int=null   
AS    
BEGIN    
    
 SELECT nStructKey,Convert(XML,cStructDescription).value('(/DisplayName/node())[1]', 'nvarchar(max)') as cStructName   
 FROM tblContentStructure cs    
 INNER JOIN tblAudit ca ON ca.nAuditKey = cs.nAuditId    
 AND ca.nStatus = 1     
 AND cs.nStructParId= isnull(@PageId,1)    
 And cs.cStructForiegnRef <>''    
 order by nStructOrder    
END 