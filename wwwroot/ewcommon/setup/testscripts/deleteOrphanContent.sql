--Delete all products not on pages

delete from tblAudit where nAuditKey in (
select c.nAuditId from  tblContent c 
left outer join tblcontentLocation cl on (c.nContentKey = nContentId )
left outer join tblcontentRelation cr on (c.nContentKey = nContentChildId )
where cContentSchemaName = 'Product'
and cl.nStructId is null
)

delete from tblContent where nContentKey in (
select c.nContentKey from  tblContent c 
left outer join tblcontentLocation cl on (c.nContentKey = nContentId )
left outer join tblcontentRelation cr on (c.nContentKey = nContentChildId )
where cContentSchemaName = 'Product'
and cl.nStructId is null
)


--Delete all orhpan not on pages or related to a parent - this will delete all library images

delete from tblAudit where nAuditKey in (
select c.nAuditId from  tblContent c 
left outer join tblcontentLocation cl on (c.nContentKey = nContentId )
left outer join tblcontentRelation cr on (c.nContentKey = nContentChildId )
where cl.nStructId is null and cr.nContentParentId is null
)

delete from tblContent where nContentKey in (
select c.nContentKey from  tblContent c 
left outer join tblcontentLocation cl on (c.nContentKey = nContentId )
left outer join tblcontentRelation cr on (c.nContentKey = nContentChildId )
where cl.nStructId is null and cr.nContentParentId is null
)