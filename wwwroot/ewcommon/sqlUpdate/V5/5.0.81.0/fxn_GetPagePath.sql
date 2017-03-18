

	CREATE FUNCTION [dbo].[fxn_getPagePath](@nStructId int)
	RETURNS nvarchar(800)  
	BEGIN 
		
	DECLARE @pagelist varchar(800), @page varchar(800), @t_Menu_pk integer
		
		SET @t_Menu_pk = @nStructId;
		SET @pagelist = ''

		WHILE (@t_Menu_pk <> 0) --Keep going thru until we have exhasted back to the highest level.
		BEGIN
			
			SELECT @t_Menu_pk = nStructParId, @page = cStructName FROM tblContentStructure WHERE nStructKey = @t_Menu_pk
			if (@t_Menu_pk <> 0)
			BEGIN
				SET @pagelist = @page + ' / ' + @pagelist
			END
		END
		/*set @pagelist =  'test' + '2'*/
		RETURN @pagelist

	END



