ALTER    PROCEDURE [dbo].[spGetUserSessionActivity]
(
	@UserId int
)
AS
BEGIN
SELECT		@UserId AS nDirKey,
			d.cDirName,
			l.cSessionId,
			dbo.fxn_SessionsStart(l.cSessionId)AS dSessionStart,
			dbo.fxn_SessionsLength(l.cSessionId) AS nSessionSeconds
FROM        tblActivityLog l
				INNER JOIN tblDirectory d
					ON l.nUserDirId = d.nDirKey AND l.nUserDirId = @UserId
				INNER JOIN
					(								-- This subtable limits the number of calls to fxn_SessionsUser
						SELECT	cSessionId
						FROM	tblActivityLog
						WHERE	@UserId=nuserdirid
						GROUP BY cSessionId
						HAVING dbo.fxn_SessionsUser(cSessionId) = @UserId
					) usersession
					ON usersession.cSessionId = l.cSessionId
GROUP BY	cDirName,l.cSessionId
ORDER BY	dSessionStart DESC
END
