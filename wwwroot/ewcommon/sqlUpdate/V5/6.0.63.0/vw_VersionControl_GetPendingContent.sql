
CREATE VIEW [dbo].[vw_VersionControl_GetPendingContent]
AS
SELECT        TOP (100) PERCENT a.nStatus AS status, c.nContentKey AS id, list.nContentVersionKey AS versionid, list.cContentName AS VersionName, 
c.cContentSchemaName AS type, a.dUpdateDate AS Last_Updated, d.nDirKey AS userid, 
                         d.cDirName AS username, d.cDirXml AS UserXml, c.cContentXmlBrief AS ContentXml, list.nVersion AS Version, CASE WHEN list.nVersion = c.nVersion THEN NULL ELSE CAST(c.nVersion AS nvarchar) END AS currentLiveVersion, 
                         c.cContentName, dbo.tblContentStructure.cStructName AS page, dbo.tblContentStructure.nStructKey AS pageid
						 ,ISNULL(CONVERT(XML, c.cContentXmlDetail).value('(/Content/ProductID)[1]', 'varchar(50)'),0)  as ContentId,
						 (select cContentName from tblContent where nContentKey = ISNULL(CONVERT(XML, c.cContentXmlDetail).value('(/Content/ProductID)[1]', 'varchar(50)'),0)) As cContentName1
FROM            (SELECT        c.nContentKey, c.nVersion, c.cContentName, c.nAuditId, NULL AS nContentVersionKey
                          FROM            dbo.tblContent AS c INNER JOIN
                                                    dbo.tblAudit AS a ON c.nAuditId = a.nAuditKey
                          WHERE        (a.nStatus = 3) OR
                                                    (a.nStatus = 4)
                          UNION
                          SELECT        c.nContentKey, v.nVersion, v.cContentName, v.nAuditId, v.nContentVersionKey
                          FROM            dbo.tblContentVersions AS v INNER JOIN
                                                   dbo.tblAudit AS a ON v.nAuditId = a.nAuditKey LEFT OUTER JOIN
                                                   dbo.tblContent AS c ON c.nContentKey = v.nContentPrimaryId
                          WHERE        (a.nStatus = 3) OR
                                                   (a.nStatus = 4)) AS list INNER JOIN
                         dbo.tblContent AS c ON list.nContentKey = c.nContentKey INNER JOIN
                         dbo.tblAudit AS a ON list.nAuditId = a.nAuditKey LEFT OUTER JOIN
                         dbo.tblDirectory AS d ON a.nUpdateDirId = d.nDirKey LEFT OUTER JOIN
                         dbo.tblContentLocation ON list.nContentKey = dbo.tblContentLocation.nContentId AND dbo.tblContentLocation.bPrimary = 1 LEFT OUTER JOIN
                         dbo.tblContentStructure ON dbo.tblContentLocation.nStructId = dbo.tblContentStructure.nStructKey
--WHERE        (list.nVersion = 0) 
--                          OR (list.nVersion > CASE WHEN list.nVersion = c.nVersion THEN NULL ELSE CAST(c.nVersion AS nvarchar) END)
WHERE cContentSchemaName='Review'
ORDER BY Last_Updated DESC



