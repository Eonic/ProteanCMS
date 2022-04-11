/****** Object:  Table [dbo].[tblXmlCache]    Script Date: 02/06/2009 12:35:26 ******/
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[tblXmlCache]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
DROP TABLE [dbo].[tblXmlCache]

/****** Object:  Table [dbo].[tblXmlCache]    Script Date: 02/06/2009 12:35:26 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[tblXmlCache]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[tblXmlCache](
	[nCacheKey] [int] IDENTITY(1,1) NOT NULL,
	[cCacheSessionID] [nchar](64) NULL,
	[nCacheDirId] [int] NULL,
	[dCacheDate] [datetime] NULL CONSTRAINT [DF_tblXmlCache_dCacheDate]  DEFAULT (getdate()),
	[cCacheStructure] [ntext] NULL,
 CONSTRAINT [PK_tblXmlCache] PRIMARY KEY CLUSTERED 
(
	[nCacheKey] ASC
) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END

