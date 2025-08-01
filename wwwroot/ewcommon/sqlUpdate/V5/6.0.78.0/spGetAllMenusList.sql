CREATE PROCEDURE [dbo].[spGetAllMenusList]          
--@nStructId int          
AS    

BEGIN   


	 SELECT
		dbo.fxn_getContentParents(c.nContentKey) AS parId,cl.nContentid,  
		c.cContentName, c.cContentXmlBrief,
		a.nStatus as status, 
		a.nInsertDirId as owner, CL.cPosition as position
	FROM tblContent c
	INNER JOIN tblContentLocation cl ON c.nContentKey = cl.nContentId
	INNER JOIN tblAudit a ON c.nAuditId = a.nAuditKey
	WHERE c.cContentName IN ('PageTitle', 'MetaDescription')


END