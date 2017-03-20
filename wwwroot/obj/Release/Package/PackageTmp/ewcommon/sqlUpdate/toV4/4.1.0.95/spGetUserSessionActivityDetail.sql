ALTER PROCEDURE spGetUserSessionActivityDetail
(
	@SessionId nvarchar(255)
)
AS
BEGIN
SELECT  cSessionId,  dbo.fxn_SessionsUser(@SessionId) AS nDirKey,
((SELECT cDirName FROM tblDirectory WHERE nDirKey = dbo.fxn_SessionsUser(@SessionId))) AS cDirName,
dDateTime , nActivityType, 
--Activity Type
CASE 
          
WHEN nActivityType = 1 THEN 'Logon'
WHEN nActivityType = 2 THEN 'PageViewed'
WHEN nActivityType = 3 THEN 'Email'
WHEN nActivityType = 4 THEN 'Logoff'
WHEN nActivityType = 5 THEN 'Alert'
WHEN nActivityType = 6 THEN 'ContentDetailViewed'
WHEN nActivityType = 7 THEN 'SessionStart'
WHEN nActivityType = 8 THEN 'SessionEnd'
WHEN nActivityType = 9 THEN 'SessionContinuation'
WHEN nActivityType = 10 THEN 'Register'
WHEN nActivityType = 11 THEN 'DocumentDownload'
            
WHEN nActivityType = 40 THEN 'ContentAdded'
WHEN nActivityType = 41 THEN 'ContentEdited'
WHEN nActivityType = 42 THEN 'ContentHidden'
WHEN nActivityType = 43 THEN 'ContentDeleted'

WHEN nActivityType = 60 THEN 'PageAdded'
WHEN nActivityType = 61 THEN 'PageEdited'
WHEN nActivityType = 62 THEN 'PageHidden'
WHEN nActivityType = 63 THEN 'PageDeleted'

WHEN nActivityType = 80 THEN 'NewsLetterSent'
           
WHEN nActivityType = 98 THEN 'Custom1'
WHEN nActivityType = 99 THEN 'Custom2'
ELSE 'Undefined'
END
AS cActivityType


,nStructId AS nPrimaryId, dbo.fxn_GetActivityDetail(nActivityType,nStructId) AS cPrimaryDetail, nArtId AS nSecondaryId, cActivityDetail

FROM 	tblActivityLog
WHERE cSessionId = @SessionID
ORDER BY dDateTime
	
END