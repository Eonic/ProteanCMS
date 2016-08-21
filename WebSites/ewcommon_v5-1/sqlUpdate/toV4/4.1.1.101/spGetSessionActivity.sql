ALTER PROCEDURE [dbo].[spGetSessionActivity]
	-- Add the parameters for the stored procedure here
 @FROM datetime,
 @TO datetime,
 @GROUPS nvarchar(255)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @Sessions table (nDirKey int, cDirName nvarchar(255), cSessionID nvarchar(255), 
dSessionStart datetime, nNoPages int, nSessionSeconds int)

INSERT INTO @Sessions (cSessionID, nDirKey, cDirName, dSessionStart, nNoPages, nSessionSeconds) 

	SELECT DISTINCT 
		tblActivityLog_1.cSessionId AS cSessID, 
		tblActivityLog_1.nUserDirId AS nDirKey, 
		tblDirectory.cDirName AS cDirName,

            (SELECT MIN(dDateTime) AS dSessionStart
				FROM tblActivityLog AS tblActivityLog_2
                WHERE (cSessionId = tblActivityLog_1.cSessionId) 
                AND (nActivityType = 2)) AS dSessionStart,

			(SELECT COUNT(nActivityKey) AS nNoPages
				FROM tblActivityLog
                WHERE (cSessionId = tblActivityLog_1.cSessionId) 
                AND (nActivityType = 2)) AS cPageViewCount,
                    
            (SELECT     DATEDIFF(second, MIN(dDateTime), MAX(dDateTime)) AS nSessionSeconds
				FROM tblActivityLog AS tblActivityLog_3
                WHERE (cSessionId = tblActivityLog_1.cSessionId) 
                AND (nActivityType = 2)) AS nSessionLength
		
		FROM tblDirectoryRelation 
			INNER JOIN tblDirectory ON tblDirectoryRelation.nDirChildId = tblDirectory.nDirKey 
			RIGHT OUTER JOIN tblActivityLog AS tblActivityLog_1 ON tblDirectory.nDirKey = tblActivityLog_1.nUserDirId
		
		WHERE 
			(tblActivityLog_1.nUserDirId > 0) 
			AND 
			(tblActivityLog_1.dDateTime <= CONVERT(DATETIME, @TO, 102)) 
			AND (tblActivityLog_1.dDateTime >= CONVERT(DATETIME, @FROM, 102)) 
			AND (tblDirectoryRelation.nDirParentId IN (@GROUPS))
			
		GROUP BY tblActivityLog_1.cSessionId, tblActivityLog_1.nUserDirId, tblDirectory.cDirName


SELECT  * 
	from @Sessions
	ORDER BY dSessionStart DESC

END