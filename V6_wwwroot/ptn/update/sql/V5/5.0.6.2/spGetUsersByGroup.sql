/* 
=============================================
  Author:	    Christopher Friedenthal
  Create date: 	10/08/2011
  Description:	Filters the existing spGetUsers by membership of a group ID
=============================================
*/
CREATE PROCEDURE [dbo].[spGetUsersByGroup] (
	@nParDirId int = 0,
	@nStatus int = 99,
	@nGroup int = 1133
)
AS
BEGIN

	CREATE TABLE #tmpTblUsers
		(
		 [id] INT,
		 [Status] INT,
		 [Username] NVARCHAR(100),
		 [Password] NVARCHAR(50),
		 [UserXml] NVARCHAR(max),
		 [User_Company] NVARCHAR(100),
		 [Departments] NVARCHAR(200), --the NVARCHAR dimension might need increasing
		 [Roles] NVARCHAR(100)
		 )
	
	INSERT INTO #tmpTblUsers 
	EXEC spGetUsers @nParDirId, @nStatus
		
SELECT   
		#tmpTblUsers.[id], 
		#tmpTblUsers.[Status],
		#tmpTblUsers.[Username],
		#tmpTblUsers.[Password], 
		#tmpTblUsers.[UserXml],
		#tmpTblUsers.[User_Company],
		#tmpTblUsers.[Departments],
		#tmpTblUsers.[Roles]


FROM         tblDirectory AS tblGroups INNER JOIN
                      tblDirectoryRelation ON tblGroups.nDirKey = tblDirectoryRelation.nDirParentId INNER JOIN
                      #tmpTblUsers ON tblDirectoryRelation.nDirChildId = #tmpTblUsers.id
WHERE     (tblGroups.cDirSchema = N'Group') AND (tblGroups.nDirKey = @nGroup)


END
