
/****** Object:  StoredProcedure [dbo].[getContentStructure_Basic]    Script Date: 02/05/2009 15:30:53 ******/
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[getContentStructure_Basic]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[getContentStructure_Basic]


/****** Object:  StoredProcedure [dbo].[getContentStructure_Basic]    Script Date: 02/05/2009 15:30:53 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[getContentStructure_Basic]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'




CREATE PROCEDURE [dbo].[getContentStructure_Basic]
	-- Add the parameters for the stored procedure here
		@UserId int,
		@dateNow datetime,
		@authUsersGrp int = 0,
		@bReturnDenied bit,
		@bShowAll bit
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
	CASE
		WHEN perms.permission = 0 THEN ''DENIED''
		WHEN perms.permission = 1 THEN ''OPEN''
		WHEN perms.permission = 2 THEN ''VIEW''
		WHEN perms.permission = 3 THEN ''ADD''
		WHEN perms.permission = 4 THEN ''ADDUPDATEOWN''
		WHEN perms.permission = 5 THEN ''UPDATEALL''
		WHEN perms.permission = 6 THEN ''APPROVE''
		WHEN perms.permission = 7 THEN ''ADDUPDATEOWNPUBLISH''
		WHEN perms.permission = 8 THEN ''PUBLISH''
		WHEN perms.permission = 9 THEN ''FULL''
	END as access,	
	s.cStructLayout as layout,
	s.nCloneStructId as clone,
	'''' As accessSource,
	0 As accessSourceId 

FROM        
	tblContentStructure s
	INNER JOIN  tblAudit a 
		ON s.nAuditId = a.nAuditKey
			AND	(a.nStatus = 1 OR @bShowAll=1)
			AND (a.dPublishDate is null or a.dPublishDate = 0 or a.dPublishDate <= @dateNow OR @bShowAll=1)
			AND (a.dExpireDate is null or a.dExpireDate = 0 or a.dExpireDate >= @dateNow OR @bShowAll=1) 
	INNER JOIN 
	(
		SELECT	
				permsummary.nStructKey,
				CASE
					WHEN permsummary.actualperms = 0 THEN 1				-- no perms = OPEN
					WHEN permsummary.explicituserperms = 0 
						OR permsummary.minuserpermission=999999 THEN 0	-- implicit perms = DENIED
					ELSE permsummary.minuserpermission					-- minimum permission level found
				END as permission
		FROM
		(
			-- The perms summary table
			-- This works out how many explicit and implicit permissions exist, while also getting the lowest permission level
			
			SELECT	
					s.nStructKey,
					COUNT(p.nPermKey) as actualperms,		-- How many actual permissions were found
					COUNT(u.nDirId) AS explicituserperms,	-- How many of them were from the user or its members
					MIN(									-- Return the lowest user explicit permission level
						CASE 
							WHEN NOT(u.nDirId IS NULL)		-- Focus on explicit permissions 
							THEN p.nAccessLevel				-- Return the lowest explicit permission level for that user group
							ELSE 999999						-- Ignore implied permissions - if 999999 is returned, then we know this is an implied permission
						END
					) as minuserpermission
					
			FROM	tblContentStructure s
					LEFT JOIN	dbo.tblDirectoryPermission p 
						ON	s.nStructKey = p.nStructId
					LEFT JOIN	
						(
							SELECT	*
							FROM	dbo.fxn_getMembers(@UserId,0,''All'',0,0,@dateNow,1)
							UNION
							SELECT	nDirKey As nDirId, cDirName, cDirSchema As cType
							FROM	tblDirectory
							WHERE	nDirKey = @authUsersGrp
							UNION
							SELECT	nDirKey As nDirId, cDirName, cDirSchema As cType
							FROM	tblDirectory
							WHERE	nDirKey = @UserId							
						) u
						ON	p.nDirId = u.nDirId
			GROUP BY s.nStructKey
		) permsummary
	) perms
		ON perms.nStructKey = s.nStructKey
			AND (perms.permission <> 0 OR @bReturnDenied = 1)

ORDER BY	s.nStructParId, s.nStructOrder


' 
END

