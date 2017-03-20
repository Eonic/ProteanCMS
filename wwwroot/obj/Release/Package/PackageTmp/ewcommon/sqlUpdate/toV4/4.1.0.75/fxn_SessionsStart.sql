CREATE    FUNCTION dbo.fxn_SessionsStart (@SessionId nvarchar(255))
RETURNS datetime
AS
BEGIN

	DECLARE @SessionStart datetime


	SET @SessionStart=(
		   SELECT TOP 1 dDateTime 
		   FROM tblActivityLog SessionStart 
		   WHERE (cSessionId = @SessionId) 
		   ORDER BY dDateTime
		   )

	RETURN @SessionStart
END