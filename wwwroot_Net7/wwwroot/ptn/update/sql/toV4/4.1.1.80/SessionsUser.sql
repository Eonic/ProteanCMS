
CREATE    FUNCTION [dbo].[fxn_SessionsUser] (@SessionId nvarchar(255))
RETURNS  integer
AS  
BEGIN
DECLARE @UserId as integer 
SET @UserID = (	SELECT  TOP 1 nUserDirId 
		FROM tblActivityLog 
		WHERE (cSessionId = @SessionId) AND (nUserDirId IS NOT NULL AND NOT nUserDirId = 0)
		GROUP BY nUserDirId 
		ORDER BY COUNT(nUserDirId) DESC)
IF @UserId = NULL
SET @UserId = 0
	RETURN @UserId
END


