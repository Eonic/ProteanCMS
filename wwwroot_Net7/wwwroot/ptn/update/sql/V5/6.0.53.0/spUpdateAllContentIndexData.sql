CREATE PROCEDURE [dbo].[spUpdateAllContentIndexData] 
AS
BEGIN
DECLARE
@ID int,
@Name nchar(20)

DECLARE cursor_content CURSOR FOR
SELECT nContentKey, cContentSchemaName FROM tblContent

OPEN cursor_content
FETCH NEXT FROM cursor_content INTO @ID, @Name
PRINT('ID  ' + 'Schema Name             ' )
WHILE @@FETCH_STATUS=0
BEGIN
	PRINT(STR(@ID)+'    '+ @Name)

	EXEC [spUpdateContentIndexData]
		@cContentSchemaName = @Name,
		@contentKey = @ID

	FETCH NEXT FROM cursor_content INTO @ID, @Name

END
CLOSE cursor_content
DEALLOCATE cursor_content
END