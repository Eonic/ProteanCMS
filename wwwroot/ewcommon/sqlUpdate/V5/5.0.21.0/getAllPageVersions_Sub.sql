	CREATE PROCEDURE  [dbo].[getAllPageVersions_sub]
		-- Add the parameters for the stored procedure here
	AS
	SELECT		
			s.nStructKey, 
			s.nStructParId, 
			s.cStructName, 
			s.cUrl, 
			s.cStructDescription, 
			a.dPublishDate, 
			a.dExpireDate, 
			a.nStatus, 
			s.cStructLayout,
			s.nCloneStructId,
			s.nVersionParId,
			s.cVersionLang,
			s.cVersionDescription ,
			s.nVersionType
	FROM	tblContentStructure s
			INNER JOIN  tblAudit a 
				ON s.nAuditId = a.nAuditKey
	where s.nVersionParId > 0
	ORDER BY	s.nStructParId, s.nStructOrder