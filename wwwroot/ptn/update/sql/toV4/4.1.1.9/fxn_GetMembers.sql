
/****** Object:  UserDefinedFunction [dbo].[fxn_getMembers]    Script Date: 02/05/2009 15:30:55 ******/
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[fxn_getMembers]') AND xtype in (N'FN', N'IF', N'TF'))
DROP FUNCTION [dbo].[fxn_getMembers]


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

