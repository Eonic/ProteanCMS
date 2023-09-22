CREATE VIEW dbo.vw_GetDistinctEvents AS  
SELECT distinct [tblContentRelation].nContentParentId, [dbo].[tblContent].[nContentKey], [tblContent].cContentName,
		[dbo].[tblContent].cContentSchemaName
FROM [dbo].[tblContentRelation] INNER JOIN [dbo].[tblContent] ON [tblContentRelation].nContentParentId = [tblContent].nContentKey
WHERE [dbo].[tblContent].[cContentSchemaName] = 'Event'
