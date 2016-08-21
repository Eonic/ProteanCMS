/* 
=============================================
  Author:	Trevor Spink
  Create date: 	25/06/06
  Description:	Get a list of all users.
=============================================
*/
CREATE PROCEDURE dbo.spSearchUsers
(
	@cSearch nvarchar(255) = ''
)
AS
BEGIN
	SELECT
		users.nDirKey as id, 
		audit.nStatus as Status,  
		users.cDirName as Username, 
		users.cDirXml as UserXml,
		dbo.fxn_getUserCompanies(users.nDirKey) as Companies
	FROM dbo.tblDirectory users
		INNER JOIN dbo.tblAudit audit
			ON users.nAuditId = audit.nAuditKey AND users.cDirSchema = 'User' -- AND audit.nStatus <> 0
	WHERE users.cDirName like '%' + @cSearch + '%' or users.cDirXml like '%' + @cSearch + '%'
	ORDER BY users.cDirName 
	
END