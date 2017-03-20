CREATE VIEW dbo.vw_SessionPageCount
AS
SELECT	cSessionId, COUNT(nActivityKey) As PageCount
FROM	tblActivityLog 
WHERE	nActivityType = 2
GROUP BY cSessionId

