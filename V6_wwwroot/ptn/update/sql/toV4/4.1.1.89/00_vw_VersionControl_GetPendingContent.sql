ALTER VIEW [dbo].[vw_VersionControl_GetPendingContent] 
AS
SELECT 
		a.nStatus as status,
		c.nContentKey as id,
		list.nContentVersionKey as versionid,
		list.cContentName as Name,
		c.cContentSchemaName as Type,
		a.dUpdateDate as [Last_Updated],
		d.nDirKey As userid,
		d.cDirName As username,
		d.cDirXml As UserXml,
		c.cContentXmlBrief As ContentXml,
		list.nVersion As [Version],
		CASE 
			WHEN list.nVersion = c.nVersion
			THEN NULL
			ELSE CAST(c.nVersion AS  nvarchar)
		END As [currentLiveVersion]
FROM
		(
			SELECT	nContentKey, nVersion, cContentName, nAuditId, NULL as nContentVersionKey 
			FROM	tblContent c
					INNER JOIN tblAudit a
						ON c.nAuditId = a.nAuditKey
			WHERE	a.nStatus = 3
			UNION
			SELECT	c.nContentKey, v.nVersion, v.cContentName, v.nAuditId, v.nContentVersionKey
			FROM	dbo.tblContentVersions v
					INNER JOIN tblAudit a
						ON v.nAuditId = a.nAuditKey
					INNER JOIN dbo.tblContent c
						ON c.nContentKey = v.nContentPrimaryId
			WHERE	a.nStatus = 3
		) list
		INNER JOIN tblContent c
			ON list.nContentKey = c.nContentKey
		INNER JOIN tblAudit a
			ON list.nAuditId = a.nAuditKey
		INNER JOIN tblDirectory d
			ON a.nUpdateDirId = d.nDirKey



