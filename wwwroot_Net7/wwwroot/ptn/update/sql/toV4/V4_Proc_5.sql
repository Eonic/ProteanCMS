/****** Object:  Stored Procedure dbo.getUsersCompanyDepartments    Script Date: 07/06/2006 23:43:09 ******/
CREATE PROCEDURE dbo.getUsersCompanyAllParents
	-- Parameters : Company ID
		@UserId int,
		@parId int
AS
BEGIN
    -- Insert statements for procedure here
	select 
	d.nDirKey as id, 
	d.cDirName as name, 
	dr.nDirChildId as isMember, 
	d.cDirSchema as type
	
FROM ((
	(tblDirectory d inner join tblAudit a on nAuditId = a.nAuditKey 
	INNER JOIN tblDirectoryRelation dept2company ON d.nDirKey = dept2company.nDirChildId) 
		INNER JOIN tblDirectory company ON company.nDirKey = dept2company.nDirParentId) 
			INNER JOIN tblDirectoryRelation user2company ON company.nDirKey = user2company.nDirParentId) 
INNER JOIN tblDirectory users ON users.nDirKey = user2company.nDirChildId 
left outer join tblDirectoryRelation dr on d.nDirKey = dr.nDirParentId and dr.nDirChildId = @UserId

WHERE d.cDirSchema <> 'User' 
AND company.cDirSchema = 'Company' 
AND users.cDirSchema = 'User' 
AND users.nDirKey= @UserId 

UNION

	select 
	d.nDirKey as id, 
	d.cDirName as name, 
	user2group.nDirChildId as isMember, 
	d.cDirSchema as type
	from 
	(tblDirectory d inner join tblAudit a on nAuditId = a.nAuditKey 
	left outer JOIN tblDirectoryRelation dept2company 
	ON d.nDirKey = dept2company.nDirChildId) 
	
	left outer join tblDirectoryRelation user2group on user2group.nDirParentId = d.nDirKey and user2group.nDirChildId = @UserId

WHERE not (d.cDirSchema in('User','Company'))
and dept2company.nRelKey is null
order by type

END