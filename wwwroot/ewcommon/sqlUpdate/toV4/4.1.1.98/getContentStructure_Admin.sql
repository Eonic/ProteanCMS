ALTER PROCEDURE [dbo].[getContentStructure_Admin]
	-- Add the parameters for the stored procedure here
AS
SELECT		
		s.nStructKey as id, 
		s.nStructParId as parId, 
		s.cStructName as name, 
		s.cUrl as url, 
		s.cStructDescription as Description, 
		a.dPublishDate as publish, 
		a.dExpireDate as expire, 
		a.nStatus as status, 
		'ADMIN' as access,	
		s.cStructLayout as layout,
		s.nCloneStructId as clone,
		'' As accessSource,
		0 As accessSourceId, 
		s.nVersionParId as vParId
FROM	tblContentStructure s
		INNER JOIN  tblAudit a 
		ON s.nAuditId = a.nAuditKey
where s.nVersionParId is null or s.nVersionParId = 0
ORDER BY	s.nStructParId, s.nStructOrder