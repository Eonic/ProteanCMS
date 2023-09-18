CREATE VIEW dbo.vw_SessionNonNullSummary AS
SELECT	TOP 100 PERCENT cSessionId,nUserDirId,COUNT(nUserDirId) As nCount,MIN(dDateTime) As SessionStart, MAX(dDateTime) As SessionEnd
FROM	tblActivityLog
WHERE	NOT(nUSerDirID = 0 OR nUSerDirID IS NULL)
GROUP BY cSessionId  ,nUserDirId
ORDER BY cSessionId,nUserDirId,COUNT(nUserDirId)


