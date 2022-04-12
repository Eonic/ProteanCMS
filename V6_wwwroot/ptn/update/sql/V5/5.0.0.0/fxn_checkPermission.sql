ALTER FUNCTION [dbo].[fxn_checkPermission] (@nPageId int, @nUserId int, @nAuthUserGrp int = 0)  
RETURNS nVarChar(255)
AS  
BEGIN
	
DECLARE @value int 
DECLARE @permCount int 
DECLARE @level nVarChar(50)
DECLARE @nPageParentId int 
DECLARE @cPermParent nVarchar(255)
DECLARE @nPermGrandParent int, @nGrandParent int, @cGrandParent nVarchar(255)
DECLARE @nPermParent int, @nParent int, @cParent nVarchar(255) 
DECLARE @nPermUser int 
	--test if page has any permissions at all
	SET @level = ''
	SET @cPermParent = ''	
	SET @permCount = (
		SELECT 	count(*) 
		FROM 	tblDirectoryPermission p 
		WHERE 	p.nStructId = @nPageId
	)
	
	If @permCount> 0
		BEGIN
			------------------------------------------------
			--------- Get grandparent permissions ----------
			------------------------------------------------
			SET @nGrandParent = (
				SELECT 	TOP 1 p.nDirId  
				FROM 	tblDirectoryPermission p 
					INNER JOIN tblDirectoryRelation dr2 ON dr2.nDirParentId = p.nDirId AND p.nStructId = @nPageId
					INNER JOIN tblDirectoryRelation dr ON dr.nDirParentId = dr2.nDirChildId AND dr.nDirChildId = @nUserId
				ORDER BY p.nAccessLevel 
				)
			IF @nGrandParent IS NOT NULL	
			BEGIN	
				SET @nPermGrandParent = (
					SELECT 	MAX(p.nAccessLevel)
					FROM 	tblDirectoryPermission p 
					WHERE	p.nDirId = @nGrandParent 
							AND p.nStructId = @nPageId
						)
				SET @cGrandParent = (SELECT cDirName FROM tblDirectory WHERE nDirKey = @nGrandParent)
			END
			
			IF @nPermGrandParent <> 0 OR @nPermGrandParent IS NULL
			BEGIN
			
				------------------------------------------------
				------------ Get parent permissions ------------
				------------------------------------------------
				SET @nParent = (
					SELECT 	TOP 1 p.nDirId  
					FROM 	tblDirectoryPermission p 
						INNER JOIN tblDirectoryRelation dr 
							ON dr.nDirParentId = p.nDirId 
								AND p.nStructId = @nPageId 
								AND dr.nDirChildId = @nUserId
					ORDER BY p.nAccessLevel 
					)
	
				IF @nParent IS NOT NULL	
				BEGIN	
					SET @nPermParent = (
						SELECT 	MAX(p.nAccessLevel)
						FROM 	tblDirectoryPermission p 
						WHERE	p.nDirId = @nParent 
								AND p.nStructId = @nPageId
							)
					SET @cParent = (SELECT cDirName FROM tblDirectory WHERE nDirKey = @nParent)
				END
				IF @nPermParent <> 0 OR @nPermParent IS NULL
				BEGIN
					------------------------------------------------
					------------ Get user permissions --------------
					------------------------------------------------
					IF @nUserId > 0 
					BEGIN
					SET @nPermUser = (
						SELECT 	MAX(p.nAccessLevel)
						FROM 	tblDirectoryPermission p 
						WHERE	(p.nDirId = @nUserId or p.nDirId = @nAuthUserGrp)
								AND p.nStructId = @nPageId
							)
					END
				END
			END
			------------------------------------------------
			------- Work out which permission to use -------
			------------------------------------------------
			IF @nGrandParent IS NOT NULL 
			BEGIN
				SET @value = @nPermGrandParent
				SET @cPermParent = @cGrandParent
			END
			IF @nParent IS NOT NULL AND (@nPermParent >= @value OR @value IS NULL)
			BEGIN
				SET @value = @nPermParent
				SET @cPermParent = @cParent
			END
			IF @nPermUser IS NOT NULL AND (@nPermUser >= @value OR @value IS NULL)
			BEGIN
				SET @value = @nPermUser
				--SET @value = @nPermParent
				SET @cPermParent = 'User'
			END
		END
	------------------------------------------------
	------ No permissions - go to parent page ------
	------------------------------------------------
	ELSE
	BEGIN
		SET @value=1
		--test permissions of parents if ok
		SET @nPageParentId = (SELECT nStructParId from tblContentStructure where nStructKey = @nPageId)
			IF @nPageParentId > 0
				SET @level = dbo.fxn_checkPermission(@nPageParentId,@nUserId,@nAuthUserGrp)
	END
	------------------------------------------------
	------ Check Level -----------------------------
	------------------------------------------------	
	IF @level <> ''
		BEGIN
			if @level <> 'OPEN' 
				BEGIN
					if LEFT(@level,9) <> 'INHERITED'
						SET @level = 'INHERITED ' + @level
				END
		END
	ELSE
	BEGIN
		if @value is NULL
			SET @level = 'IMPLIED DENIED'
		IF @value = 0
			SET @level = 'DENIED'
		IF @value = 1
			SET @level = 'OPEN'
		IF @value = 2
			SET @level = 'VIEW' 
		IF @value = 3
			SET @level = 'ADD'
		IF @value = 4
			SET @level = 'ADDUPDATEOWN'
		IF @value = 5
			SET @level = 'UPDATEALL' 
		IF @value = 6
			SET @level = 'APPROVE' 
		IF @value = 7
			SET @level = 'ADDUPDATEOWNPUBLISH' 
		IF @value = 8
			SET @level = 'PUBLISH' 
		IF @value = 9
			SET @level = 'FULL'
		IF @cPermParent <> ''
			SET @level = @level + ' by ' + @cPermParent 
	END
	
	
	return @level
	
END
