
/****** Object:  StoredProcedure [dbo].[getContentStructure_Admin]    Script Date: 02/05/2009 15:30:53 ******/
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[getContentStructure_Admin]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[getContentStructure_Admin]


/****** Object:  StoredProcedure [dbo].[getContentStructure_Admin]    Script Date: 02/05/2009 15:30:53 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[getContentStructure_Admin]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'



CREATE PROCEDURE [dbo].[getContentStructure_Admin]
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
		''ADMIN'' as access,	
		s.cStructLayout as layout,
		s.nCloneStructId as clone,
		'''' As accessSource,
		0 As accessSourceId 
FROM	tblContentStructure s
		INNER JOIN  tblAudit a 
			ON s.nAuditId = a.nAuditKey
ORDER BY	s.nStructParId, s.nStructOrder

' 
END

