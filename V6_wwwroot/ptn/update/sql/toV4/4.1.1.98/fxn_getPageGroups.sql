	CREATE FUNCTION [dbo].[fxn_getPageGroups](@nStructId int)
	RETURNS nvarchar(128)  
	BEGIN 
	DECLARE @rolelist varchar(255), @role varchar(255)
	DECLARE roles CURSOR FOR 
			
		SELECT d.cDirName as name 
		from tblDirectoryPermission p 
		inner join tblDirectory d on d.nDirKey = p.nDirId 
		where p.nStructId = @nStructId and p.nAccessLevel = 2
		order by d.cDirSchema
		
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
