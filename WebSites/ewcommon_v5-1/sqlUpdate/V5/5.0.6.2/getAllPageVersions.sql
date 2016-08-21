/****** Object:  StoredProcedure [dbo].[getAllPageVersions]    Script Date: 04/19/2012 23:36:30 ******/

ALTER   PROCEDURE  [dbo].[getAllPageVersions]
		-- Add the parameters for the stored procedure here
	AS
	
	DECLARE @tempResult TABLE 
	(
			nStructKey int, 
			nStructParId int, 
			cStructName nvarchar(800), 
			cUrl nvarchar(255), 
			cStructDescription nvarchar(800), 
			dPublishDate datetime, 
			dExpireDate datetime, 
			nStatus int,
			cStructLayout nvarchar(255),
			nCloneStructId int,
			nVersionParId int,
			cVersionLang nvarchar(50),
			cVersionDescription nvarchar(800),
			nVersionType nvarchar(50)
	)
	
INSERT @tempResult Exec getAllPageVersions_sub;

SELECT
			nStructKey as id, 
			nStructParId as parId, 
			cStructName as name, 
			cUrl as url, 
			cStructDescription as Description, 
			dPublishDate as publish, 
			dExpireDate as expire, 
			nStatus as status, 
			'ADMIN' as access,	
			cStructLayout as layout,
			nCloneStructId as clone,
			nVersionParId as vParId,
			cVersionLang as lang,
			cVersionDescription as [desc],
			nVersionType as verType,
			dbo.fxn_getPageGroups(nStructKey) as Groups
FROM @tempResult