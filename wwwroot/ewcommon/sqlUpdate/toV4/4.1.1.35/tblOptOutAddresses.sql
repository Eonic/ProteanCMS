/****** Object:  Table [dbo].[tblOptOutAddresses]    Script Date: 11/13/2009 09:54:29 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

IF NOT EXISTS(SELECT name FROM sys.tables WHERE name = 'tblOptOutAddresses')
CREATE TABLE [dbo].[tblOptOutAddresses](
	[EmailAddress] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_tblOptOutAddresses] PRIMARY KEY CLUSTERED 
(
	[EmailAddress] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]



