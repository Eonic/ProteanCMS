--Delete all products not on pages

delete from tblAudit where nAuditKey in (
select nAuditId from  tblContentLocation where nContentLocationKey  IN (
SELECT
   MAX(nContentLocationKey) as clkey
FROM
    tblContentLocation
GROUP BY
    nContentId, nStructId
HAVING 
    COUNT(*) > 1
)
)

delete from tblContentLocation where nContentLocationKey in (
select nContentLocationKey from  tblContentLocation where nContentLocationKey  IN (
SELECT
   MAX(nContentLocationKey) as clkey
FROM
    tblContentLocation
GROUP BY
    nContentId, nStructId
HAVING 
    COUNT(*) > 1
)
)