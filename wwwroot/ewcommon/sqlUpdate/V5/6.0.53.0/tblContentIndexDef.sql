

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblContentIndexDef]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN

CREATE TABLE [dbo].[tblContentIndexDef](
	[nContentIndexDefKey] [int] IDENTITY(1,1) NOT NULL,
	[nContentIndexDataType] [int] NOT NULL,
	[cContentSchemaName] [nchar](255) NOT NULL,
	[cDefinitionName] [nvarchar](50) NOT NULL,
	[cContentValueXpath] [nvarchar](800) NOT NULL,
	[bBriefNotDetail] [bit] NULL,
	[nKeywordGroupName] [nvarchar](50) NULL
) ON [PRIMARY]
END


