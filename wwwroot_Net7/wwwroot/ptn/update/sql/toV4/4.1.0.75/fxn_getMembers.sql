CREATE FUNCTION [dbo].[fxn_getMembers] (@nParid int, @level int = 99, @cReturnType nvarchar(32) = 'All', @bIncludeInactive bit = 0, @bIncludeExpired bit = 0, @dDateNow datetime, @bGetParents bit = 0)  
RETURNS @getMembers TABLE (nDirId int,  cDirname nVarchar(256), cType nvarchar(200)) AS  
BEGIN 
	INSERT INTO @getMembers (nDirId,  cDirname, cType)
		SELECT DISTINCT nDirId,cDirname,cType
		FROM dbo.fxn_getMembers_Sub (@nParid, @level,@cReturnType,@bIncludeInactive, @bIncludeExpired,@dDateNow,@bGetParents)
		ORDER BY cType, cDirname
	RETURN
END