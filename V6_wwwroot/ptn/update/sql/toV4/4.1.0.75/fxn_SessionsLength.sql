CREATE   FUNCTION dbo.fxn_SessionsLength (@SessionId nvarchar(255))
RETURNS integer
AS
BEGIN
	DECLARE @SessionSeconds int 
	DECLARE @SessionStart datetime
	DECLARE @SessionEnd datetime

	SET @SessionStart=(
		   SELECT TOP 1 dDateTime 
		   FROM tblActivityLog SessionStart 
		   WHERE (cSessionId = @SessionId) 
		   ORDER BY dDateTime
		   )
	SET @SessionEnd=(
		   SELECT TOP 1 dDateTime 
		   FROM tblActivityLog SessionStart 
		   WHERE (cSessionId = @SessionId) 
		   ORDER BY dDateTime DESC
		   )
	SET @SessionSeconds = DATEDIFF(second, @SessionStart,@SessionEnd )

	RETURN @SessionSeconds
END