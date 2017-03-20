/* 
=============================================
  Author:	Ali Granger
  Create date: 	09/06/06
  Description:	Get a list of users from a company.
=============================================
*/
Create PROCEDURE dbo.spGetCompanyUsersActive (
	@nDirId int
)
AS
BEGIN
	SELECT
		users.nDirKey as id, 
		dbo.fxn_getStatus(audit.nAuditKey,getdate()) as Status,  
		users.cDirName as Username, 
		users.cDirPassword as Password, 
		users.cDirXml as UserXml,
		dbo.fxn_getUserCompanies(users.nDirKey) as User_Company,
		dbo.fxn_getUserDepts(users.nDirKey,0) as Departments, 
		dbo.fxn_getUserRoles(users.nDirKey) as Roles 
	FROM dbo.tblDirectory users
		INNER JOIN dbo.tblDirectoryRelation users2company
			ON users2company.nDirParentId =  @nDirId  AND users.cDirSchema = 'User' AND users2company.nDirChildId = users.nDirKey
		INNER JOIN dbo.tblAudit audit
			ON users.nAuditId = audit.nAuditKey AND (audit.nStatus =1 or audit.nStatus = -1) 
	ORDER BY users.cDirName 
END