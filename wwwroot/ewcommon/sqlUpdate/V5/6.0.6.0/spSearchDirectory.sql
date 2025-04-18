
/* 
=============================================
  Author:	Trevor Spink
  Create date: 	25/06/06
  Description:	Get a list of all users.
=============================================
*/
Create PROCEDURE [dbo].[spSearchDirectory]
(
	@cSearch nvarchar(255) = '',
	@cSchemaName nvarchar(50)
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
			ON users.nAuditId = audit.nAuditKey AND users.cDirSchema = @cSchemaName -- AND audit.nStatus <> 0
	WHERE users.cDirName like '%' + @cSearch + '%' or users.cDirXml like '%' + @cSearch + '%'
	ORDER BY users.cDirName 
	
END