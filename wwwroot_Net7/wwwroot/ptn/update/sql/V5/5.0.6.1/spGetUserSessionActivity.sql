ALTER PROCEDURE [dbo].[spGetUserSessionActivity]
(
	@UserId int
)
AS
BEGIN

SELECT     @UserId AS nDirKey,
((SELECT cDirName FROM tblDirectory WHERE nDirKey = @UserId)) AS cDirName
,dbo.fxn_SessionsStart(cSessionId)AS dSessionStart,dbo.fxn_SessionsLength(cSessionId) AS nSessionSeconds,cSessionId
FROM         tblActivityLog
WHERE dbo.fxn_SessionsUser(cSessionId) = @UserId
GROUP BY cSessionId
ORDER BY dbo.fxn_SessionsStart(cSessionId) DESC
END