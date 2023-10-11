--Delete all contentlocations without content

delete from tblAudit where nAuditKey in (
select cl.nAuditId from tblContentLocation cl
left outer join tblContent c on c.nContentKey = cl.nContentId
where nContentKey is null
)

delete from tblContentLocation where nContentLocationKey in (
select nContentLocationKey from tblContentLocation cl
left outer join tblContent c on c.nContentKey = cl.nContentId
where nContentKey is null
)


