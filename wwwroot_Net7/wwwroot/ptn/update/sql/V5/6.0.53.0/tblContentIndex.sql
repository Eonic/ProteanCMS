

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblContentIndex]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN

CREATE TABLE [dbo].[tblContentIndex](
	[nContentIndexKey] [int] IDENTITY(1,1) NOT NULL,
	[nContentId] [int] NULL,
	[nContentIndexDefinitionKey] [int] NULL,
	[dDateValue] [datetime] NULL,
	[nNumberValue] [numeric](18, 0) NULL,
	[cTextValue] [nvarchar](50) NULL,
	[nLookupId] [int] NULL
) ON [PRIMARY]

End


