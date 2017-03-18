
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


