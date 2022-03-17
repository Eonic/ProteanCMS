
CREATE  FUNCTION [dbo].[fxn_getContentParents](@nContentID int)

RETURNS nvarchar(128) 

BEGIN 

DECLARE @parentlist varchar(255), @parent varchar(255)

DECLARE parents CURSOR FOR 

SELECT     nStructId
FROM         tblContentLocation
WHERE     (nContentId = @nContentID) AND (bPrimary = 1)

SET @parentlist = ''

OPEN parents

FETCH NEXT FROM parents INTO @parent 

WHILE @@FETCH_STATUS = 0

BEGIN


SET @parentlist = @parentlist + @parent

FETCH NEXT FROM parents INTO @parent 

IF @@FETCH_STATUS = 0

BEGIN

SET @parentlist = @parentlist + ','

END

END

CLOSE parents

DEALLOCATE parents

RETURN @parentlist 

END


