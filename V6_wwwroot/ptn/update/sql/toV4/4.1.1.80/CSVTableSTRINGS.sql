CREATE FUNCTION [dbo].[fxn_CSVTableSTRINGS] (@CSVStrings nvarchar(255))
RETURNS  @IDS TABLE (value nvarchar(255))
AS  
BEGIN
	DECLARE @position int
	SET @position = 1
	SET @CSVStrings = @CSVStrings + ',' -- To make sure we get the last one
	WHILE (@position > 0)
		BEGIN
			INSERT INTO @IDS (value) VALUES(substring(@CSVStrings, 1, patindex('%,%', @CSVStrings) - 1) )
			SET @CSVStrings = stuff(@CSVStrings, 1, patindex('%,%', @CSVStrings)  , NULL)
			SET @position = patindex('%,%', @CSVStrings)
		END 
	--SELECT * FROM @IDS
	RETURN
END


