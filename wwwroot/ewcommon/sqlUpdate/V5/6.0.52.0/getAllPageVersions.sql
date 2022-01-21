ALTER   PROCEDURE  [dbo].[getAllPageVersions]
		-- Add the parameters for the stored procedure here
	AS
	
	DECLARE @tempResult TABLE 
	(
			nStructKey int, 
			nStructParId int, 
			cStructName nvarchar(4000), 
			cUrl nvarchar(4000), 
			cStructDescription nText, 
			dPublishDate datetime, 
			dExpireDate datetime, 
			nStatus int,
			cStructLayout nvarchar(255),
			nCloneStructId int,
			nVersionParId int,
			cVersionLang nvarchar(50),
			cVersionDescription nvarchar(4000),
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
			stuff(
			   (
			   		SELECT d.cDirName as name 
		from tblDirectoryPermission p 
		inner join tblDirectory d on d.nDirKey = p.nDirId 
		where p.nStructId = nStructKey and p.nAccessLevel = 2
				   for xml path(''), type
			   ).value('.', 'nvarchar(max)')
			  , 1, 1, '') as Groups
			-- dbo.fxn_getPageGroups(nStructKey) as Groups
FROM @tempResult