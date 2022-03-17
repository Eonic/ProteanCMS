CREATE FUNCTION dbo.fxn_getUserRoles(@nDirId int)
RETURNS nvarchar(128)  
BEGIN 
DECLARE @rolelist varchar(255), @role varchar(255)
DECLARE roles CURSOR FOR 
	SELECT  DISTINCT dirroles.cDirName
	FROM	tblDirectoryRelation user2role
			INNER JOIN tblDirectory dirroles 
				ON user2role.nDirChildId = @nDirId
					AND user2role.nDirParentId = dirroles.nDirKey 
					AND dirroles.cDirSchema = 'Role'
	ORDER BY dirroles.cDirName
	SET @rolelist = ''
	OPEN roles 
	FETCH NEXT FROM roles INTO @role 
	WHILE @@FETCH_STATUS = 0
	BEGIN
	
	   	SET @rolelist = @rolelist + @role
		FETCH NEXT FROM roles INTO @role 
		IF @@FETCH_STATUS = 0
		BEGIN
			SET @rolelist = @rolelist + ', '
		END
	END
	CLOSE roles 
	DEALLOCATE roles 
	RETURN @rolelist 
END