/* 
=============================================
  Author:	Ali Granger
  Create date: 	09/06/06
  Description:	Get a list of all users.
=============================================
*/
CREATE PROCEDURE dbo.spGetAllUsersInActive
AS
BEGIN
	SELECT
		users.nDirKey as id, 
		dbo.fxn_getStatus(audit.nAuditKey,getdate()) as Status,  
		users.cDirName as Username, 
		users.cDirPassword as Password, 
		users.cDirXml as UserXml,
		dbo.fxn_getUserCompanies(users.nDirKey) as Companies,
		dbo.fxn_getUserDepts(users.nDirKey,0) as Departments
	FROM dbo.tblDirectory users
		INNER JOIN dbo.tblAudit audit
			ON users.nAuditId = audit.nAuditKey AND users.cDirSchema = 'User' AND audit.nStatus = 0
	ORDER BY users.cDirName 
END