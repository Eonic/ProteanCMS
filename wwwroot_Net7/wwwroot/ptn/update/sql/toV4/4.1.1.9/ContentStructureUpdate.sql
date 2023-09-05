/****** Object:  StoredProcedure [dbo].[getContentStructure_v2]    Script Date: 02/05/2009 15:30:54 ******/
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[getContentStructure_v2]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
PRINT 'getContentStructure_v2 already exists which may mean that you''ve already run this upgrade script'
DROP PROCEDURE [dbo].[getContentStructure_v2]
END

/****** Object:  StoredProcedure [dbo].[getContentStructure_Basic]    Script Date: 02/05/2009 15:30:53 ******/
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[getContentStructure_Basic]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[getContentStructure_Basic]

/****** Object:  StoredProcedure [dbo].[getContentStructure_Enumerate]    Script Date: 02/05/2009 15:30:54 ******/
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[getContentStructure_Enumerate]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[getContentStructure_Enumerate]

/****** Object:  StoredProcedure [dbo].[getContentStructure_Admin]    Script Date: 02/05/2009 15:30:53 ******/
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[getContentStructure_Admin]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[getContentStructure_Admin]

/****** Object:  UserDefinedFunction [dbo].[fxn_getMembers]    Script Date: 02/05/2009 15:30:55 ******/
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[fxn_getMembers]') AND xtype in (N'FN', N'IF', N'TF'))
DROP FUNCTION [dbo].[fxn_getMembers]

/****** Object:  UserDefinedFunction [dbo].[fxn_getMembers_Sub]    Script Date: 02/05/2009 15:30:55 ******/
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[fxn_getMembers_Sub]') AND xtype in (N'FN', N'IF', N'TF'))
DROP FUNCTION [dbo].[fxn_getMembers_Sub]

/****** Object:  UserDefinedFunction [dbo].[fxn_getMembers_Sub]    Script Date: 02/05/2009 15:30:55 ******/
SET ANSI_NULLS OFF

SET QUOTED_IDENTIFIER ON

IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[fxn_getMembers_Sub]') AND xtype in (N'FN', N'IF', N'TF'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[fxn_getMembers_Sub] (@nParid int, @level int = 99, @cReturnType nvarchar(32) = ''All'', @bIncludeInactive bit = 0, @bIncludeExpired bit = 0, @dDateNow datetime, @bGetParents bit = 0)  
RETURNS @getMembers TABLE (nDirId int,  cDirname nVarchar(256), cType nvarchar(200)) AS  
BEGIN 
	IF @level < 7
	BEGIN
		SET @level = @level + 1
		-- Get Children, run through one at a time, add them to the table, call the function on them.
		DECLARE @nChildId int,  @nChildName varchar(255),  @nChildType varchar(255)

		IF @bGetParents = 0

			DECLARE children CURSOR FOR 
				SELECT	-1 As nDirKey, ''Dummy First Line'' As cDirName, ''Dummy Schema'' As cDirSchema
				UNION
				SELECT	d.nDirKey, d.cDirname, d.cDirschema
				FROM		tblDirectoryRelation dr
						INNER JOIN tblDirectory d
							ON d.nDirKey=dr.nDirChildId AND dr.nDirParentId=@nParId
						INNER JOIN tblAudit a
							ON d.nAuditId = a.nAuditKey 
				WHERE	NOT(d.nDirKey IN (SELECT DISTINCT nDirId FROM @getMembers))
						AND (@bIncludeInactive=1 OR (@bIncludeInactive=0 AND ABS(a.nStatus)=1))
						AND (@bIncludeExpired=1 OR (@bIncludeExpired=0 AND (a.dExpireDate > @dDateNow OR a.dExpireDate IS NULL)))

		ELSE 

			DECLARE children CURSOR FOR 
				SELECT	-1 As nDirKey, ''Dummy First Line'' As cDirName, ''Dummy Schema'' As cDirSchema
				UNION
				SELECT	d.nDirKey, d.cDirname, d.cDirschema
				FROM		tblDirectoryRelation dr
						INNER JOIN tblDirectory d
							ON d.nDirKey=dr.nDirParentId AND dr.nDirChildId=@nParId -- bGetParents is on
						INNER JOIN tblAudit a
							ON d.nAuditId = a.nAuditKey 
				WHERE	NOT(d.nDirKey IN (SELECT DISTINCT nDirId FROM @getMembers))
						AND (@bIncludeInactive=1 OR (@bIncludeInactive=0 AND ABS(a.nStatus)=1))
						AND (@bIncludeExpired=1 OR (@bIncludeExpired=0 AND (a.dExpireDate > @dDateNow OR a.dExpireDate IS NULL)))


		OPEN children 
		FETCH NEXT FROM children INTO @nChildId,  @nChildName, @nChildType 
		WHILE @@FETCH_STATUS = 0
		BEGIN		
			FETCH NEXT FROM children INTO @nChildId,  @nChildName, @nChildType 
			IF @@FETCH_STATUS = 0
			BEGIN
				IF @cReturnType = @nChildType OR @cReturnType = ''All''
					INSERT INTO @getMembers (nDirId,  cDirname, cType) VALUES (@nChildId,  @nChildName, @nChildType)
				-- Stop unnecessary recursion for child types that do not have children
				IF  (@nChildType <> ''User'' AND @bGetParents=0) OR (@bGetParents=1 AND NOT(@nChildType IN (''Role'')))
					INSERT INTO  @getMembers (nDirId,  cDirname, cType)
						SELECT nDirId,  cDirname, cType
						FROM  dbo.fxn_getMembers_Sub (@nChildId,@level,@cReturnType, @bIncludeInactive, @bIncludeExpired, @dDateNow,@bGetParents)
			END
		END
		CLOSE children 
		DEALLOCATE children
	END
	RETURN
END' 
END

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

/****** Object:  UserDefinedFunction [dbo].[fxn_getMembers]    Script Date: 02/05/2009 15:30:55 ******/
SET ANSI_NULLS OFF

SET QUOTED_IDENTIFIER ON

IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[fxn_getMembers]') AND xtype in (N'FN', N'IF', N'TF'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[fxn_getMembers] (@nParid int, @level int = 99, @cReturnType nvarchar(32) = ''All'', @bIncludeInactive bit = 0, @bIncludeExpired bit = 0, @dDateNow datetime, @bGetParents bit = 0)  
RETURNS @getMembers TABLE (nDirId int,  cDirname nVarchar(256), cType nvarchar(200)) AS  
BEGIN 
	INSERT INTO @getMembers (nDirId,  cDirname, cType)
		SELECT DISTINCT nDirId,cDirname,cType
		FROM dbo.fxn_getMembers_Sub (@nParid, @level,@cReturnType,@bIncludeInactive, @bIncludeExpired,@dDateNow,@bGetParents)
		ORDER BY cType, cDirname
	RETURN
END' 
END

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

/****** Object:  StoredProcedure [dbo].[getContentStructure_Enumerate]    Script Date: 02/05/2009 15:30:54 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[getContentStructure_Enumerate]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'


CREATE PROCEDURE [dbo].[getContentStructure_Enumerate]
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
	d.cDirName As accessSource,
	d.nDirKey As accessSourceId 

FROM        
	tblContentStructure s
	INNER JOIN  tblAudit a 
		ON s.nAuditId = a.nAuditKey
			AND	(a.nStatus = 1 OR @bShowAll = 1)
			AND (a.dPublishDate is null or a.dPublishDate = 0 or a.dPublishDate <= @dateNow OR @bShowAll = 1)
			AND (a.dExpireDate is null or a.dExpireDate = 0 or a.dExpireDate >= @dateNow OR @bShowAll = 1) 
	INNER JOIN 
	(
		SELECT	
				permsummary.nStructKey,
				CASE
					WHEN permsummary.actualperms = 0 THEN 1				-- no perms = OPEN
					WHEN permsummary.explicituserperms = 0 
						OR permsummary.minuserpermission=999999 THEN 0	-- implicit perms = DENIED
					ELSE permsummary.minuserpermission					-- minimum permission level found
				END as permission,
				CASE
					WHEN minpermuserid > 0 THEN minpermuserid % 1000000
					ELSE NULL
				END as permissionsource
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
					) as minuserpermission,
					MIN(
							CASE WHEN u.nDirId IS NULL AND NOT(p.nPermKey IS NULL) THEN 200000000 ELSE 100000000 END -- explicit / implicit
							+ CASE WHEN p.nAccessLevel IS NULL THEN 0 ELSE 1000000 * p.nAccessLevel END
							+ CASE WHEN p.nPermKey IS NULL THEN 0  ELSE p.nDirId END
					) as minpermuserid
					
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
	LEFT JOIN tblDirectory d
		ON d.nDirKey = perms.permissionsource
ORDER BY	s.nStructParId, s.nStructOrder
' 
END

/****** Object:  StoredProcedure [dbo].[getContentStructure_v2]    Script Date: 02/05/2009 15:30:54 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[getContentStructure_v2]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'
-- =============================================
-- Author:				Trevor Spink
-- Create date:			11/03/06
-- Last Modified:		03/02/2009
-- Last modified by:	Ali Granger
-- Description:			Returns Site Structure with users access level
-- =============================================
CREATE PROCEDURE [dbo].[getContentStructure_v2]
	-- Add the parameters for the stored procedure here
		@UserId int,
		@bAdminMode bit = 0,
		@dateNow datetime,
		@authUsersGrp int = 0,
		@bReturnDenied bit = 0,
		@bShowAll bit = 0
		
AS
BEGIN
	
	SET NOCOUNT ON
	If @bAdminMode = 0 
	
		EXEC dbo.getContentStructure_Basic 
			@UserId,
			@dateNow,
			@authUsersGrp,
			@bReturnDenied,
			@bShowAll

	ELSE IF @UserId = -1

		EXEC dbo.getContentStructure_Admin

	ELSE
		
		EXEC dbo.getContentStructure_Enumerate 
			@UserId,
			@dateNow,
			@authUsersGrp,
			@bReturnDenied,
			@bShowAll		

END
' 
END


