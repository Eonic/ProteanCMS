--To get distinct tickets
CREATE VIEW dbo.vw_GetDistinctTickets AS  
SELECT distinct [tblContentRelation].nContentParentId, [tblContentRelation].nContentChildId, [dbo].[tblContent].[nContentKey], [tblContent].cContentName,
		[dbo].[tblContent].cContentSchemaName
FROM [dbo].[tblContentRelation] LEFT OUTER JOIN [dbo].[tblContent] ON [tblContentRelation].nContentParentId = [tblContent].nContentKey
WHERE [dbo].[tblContent].[cContentSchemaName] IN ('Ticket')
 