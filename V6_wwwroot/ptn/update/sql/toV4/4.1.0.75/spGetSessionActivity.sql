CREATE      PROCEDURE dbo.spGetSessionActivity (
 @FROM datetime,
 @TO datetime,
 @GROUPS nvarchar(255)
)
AS
BEGIN


DECLARE @Sessions table (SessionID nvarchar(255),UserId int, SessionStart datetime, NoPages int,SessionSeconds int)

INSERT INTO @Sessions (SessionID) SELECT * FROM dbo.fxn_SessionsInDateRange(@FROM,@TO)

UPDATE @Sessions SET 
			UserId = dbo.fxn_SessionsUser(SessionID),
			SessionStart = (
		   		SELECT TOP 1 dDateTime 
		   		FROM tblActivityLog  
		   		WHERE (cSessionId = SessionId) 
		   		ORDER BY dDateTime),
		   	SessionSeconds = dbo.fxn_SessionsLength(SessionID),
		   	NoPages = (
				SELECT Count(tblActivityLog.nActivityKey) 
				FROM tblActivityLog 
				WHERE tblActivityLog.cSessionId = SessionId AND tblActivityLog.nActivityType = 2)
--SELECT * FROM @Sessions
DECLARE @GroupTable TABLE (GroupId int)
DECLARE @UserTable TABLE( nChildId int,  nChildName varchar(255),  nChildType varchar(255))
IF @Groups = '0' OR @Groups = '' OR @Groups IS NULL
	BEGIN 
		INSERT INTO @GroupTable SELECT 0
		INSERT INTO @UserTable (nChildId, nChildName) SELECT DISTINCT SES.UserId,tblDirectory.cDirName
		FROM @Sessions SES LEFT OUTER JOIN @UserTable Ex ON SES.UserID = Ex.nChildId INNER JOIN tblDirectory ON tblDirectory.nDirKey = SES.UserId
		WHERE (Ex.nChildId IS NULL) 

	END
ELSE
	BEGIN
		Insert INTO @GroupTable (GroupId) SELECT * FROM fxn_CSVTableINTEGERS(@Groups)


		DECLARE @GroupID int
		DECLARE @Date datetime
		SET @Date = getdate()
		

		DECLARE curGroups CURSOR FOR SELECT GroupId FROM @GroupTable
	
		OPEN curGroups 
		FETCH NEXT FROM curGroups INTO @GroupID 
		WHILE (@@FETCH_STATUS = 0)
			BEGIN
				INSERT INTO @UserTable SELECT Nw.* FROM dbo.fxn_getMembers(@GroupID,0 ,'User' ,0 , 0,@Date,0 ) Nw
				LEFT OUTER JOIN @UserTable Ex ON Nw.nDirId = Ex.nChildId
				WHERE (Ex.nChildId IS NULL)

				FETCH NEXT FROM curGroups INTO @GroupID 
			END
		CLOSE curGroups 
		DEALLOCATE curGroups
END
--SELECT * FROM @GroupTable
--SELECT * FROM @UserTable
SELECT Ex.nChildId AS nDirKey, Ex.nChildName AS cDirName, SES.SessionID AS cSessionId, SES.SessionStart AS dSessionStart, SES.NoPages AS nNoPages, SES.SessionSeconds AS nSessionSeconds
FROM @UserTable Ex Left OUTER JOIN @Sessions SES ON Ex.nChildId = SES.UserID
WHERE (NOT SES.SessionId IS NULL)
ORDER BY SES.SessionStart DESC
END