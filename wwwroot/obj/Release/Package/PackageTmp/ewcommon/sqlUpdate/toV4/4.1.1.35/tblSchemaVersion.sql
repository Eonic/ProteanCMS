
/****** Object:  Table [dbo].[tblSchemaVersion]    Script Date: 12/30/2009 11:10:26 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[tblSchemaVersion](
	[nVersionKey] [int] IDENTITY(1,1) NOT NULL,
	[MajorVersion] [int] NULL,
	[MinorVersion] [int] NULL,
	[Release] [int] NULL,
	[Build] [int] NULL,
 CONSTRAINT [PK_tblSchemaVersion] PRIMARY KEY CLUSTERED 
(
	[nVersionKey] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]


