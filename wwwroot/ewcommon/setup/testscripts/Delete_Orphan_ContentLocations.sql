--Delete all contentLocations where content does not exist

delete from tblAudit where nAuditKey in (
	select cl.nAuditId from tblContentLocation cl
	left outer join tblAudit a on a.nAuditKey = cl.nAuditId
	left outer join tblContent c on c.nContentKey = cl.nContentId
	left outer join tblContentStructure cs on cs.nStructKey = cl.nStructId
	where c.nContentKey is null
)

delete from tblContentLocation where nContentLocationKey in (
	select cl.nContentLocationKey from tblContentLocation cl
	left outer join tblAudit a on a.nAuditKey = cl.nAuditId
	left outer join tblContent c on c.nContentKey = cl.nContentId
	left outer join tblContentStructure cs on cs.nStructKey = cl.nStructId
	where c.nContentKey is null
)


