
ALTER PROCEDURE [dbo].[getAllPageVersions]
	-- Add the parameters for the stored procedure here
AS
SELECT		
		s.nStructKey as id, 
		s.nStructParId as parId, 
		s.cStructName as name, 
		s.cUrl as url, 
		s.cStructDescription as [description], 
		a.dPublishDate as publish, 
		a.dExpireDate as expire, 
		a.nStatus as status, 
		'ADMIN' as access,	
		s.cStructLayout as layout,
		s.nVersionParId as vParId,
		s.cVersionLang as lang,
		s.cVersionDescription as [desc],
		s.nVersionType as verType
FROM	tblContentStructure s
		INNER JOIN  tblAudit a 
			ON s.nAuditId = a.nAuditKey
where s.nVersionParId > 0
ORDER BY	s.nStructParId, s.nStructOrder

