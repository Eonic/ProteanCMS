CREATE  FUNCTION dbo.fxn_CSVTableINTEGERS (@CSVInts nvarchar(255))
RETURNS  @IDS TABLE (value int)
AS  
BEGIN
	DECLARE @position int
	SET @position = 1
	SET @CSVInts = @CSVInts + ',' -- To make sure we get the last one
	WHILE (@position > 0)
		BEGIN
			INSERT INTO @IDS (value) VALUES(substring(@CSVInts, 1, patindex('%,%', @CSVInts) - 1) )
			SET @CSVInts = stuff(@CSVInts, 1, patindex('%,%', @CSVInts)  , NULL)
			SET @position = patindex('%,%', @CSVInts)
		END 
	--SELECT * FROM @IDS
	RETURN
END