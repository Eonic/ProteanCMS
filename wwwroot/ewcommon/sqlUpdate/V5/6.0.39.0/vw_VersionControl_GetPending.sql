USE [ptn_intotheblue_co_uk]
GO

/****** Object:  View [dbo].[vw_VersionControl_GetPendingContent]    Script Date: 16/05/2020 11:04:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[vw_VersionControl_GetPendingContent]
AS
SELECT        a.nStatus AS status, c.nContentKey AS id, list.nContentVersionKey AS versionid, list.cContentName AS VersionName, c.cContentSchemaName AS Type, a.dUpdateDate AS Last_Updated, d.nDirKey AS userid, 
                         d.cDirName AS username, d.cDirXml AS UserXml, c.cContentXmlBrief AS ContentXml, list.nVersion AS Version, CASE WHEN list.nVersion = c.nVersion THEN NULL ELSE CAST(c.nVersion AS nvarchar) END AS currentLiveVersion, 
                         c.cContentName, dbo.tblContentStructure.cStructName AS page, dbo.tblContentStructure.nStructKey AS pageid
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
                         dbo.tblAudit AS a ON list.nAuditId = a.nAuditKey INNER JOIN
                         dbo.tblDirectory AS d ON a.nUpdateDirId = d.nDirKey INNER JOIN
                         dbo.tblContentLocation ON list.nContentKey = dbo.tblContentLocation.nContentId AND dbo.tblContentLocation.bPrimary = 1 INNER JOIN
                         dbo.tblContentStructure ON dbo.tblContentLocation.nStructId = dbo.tblContentStructure.nStructKey
GO

