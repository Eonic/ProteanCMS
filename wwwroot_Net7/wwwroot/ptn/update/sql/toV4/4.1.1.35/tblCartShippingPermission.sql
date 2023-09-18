/****** Object:  Table [dbo].[tblCartShippingPermission]    Script Date: 01/23/2010 10:15:17 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

if not Exists(select * from sys.tables where Name = N'tblCartShippingPermission')
BEGIN
	CREATE TABLE [dbo].[tblCartShippingPermission](
	[nCartShippingPermissionKey] [int] IDENTITY(1,1) NOT NULL,
	[nShippingMethodId] [int] NOT NULL,
	[nDirId] [int] NOT NULL,
	[nAuditId] [int] NOT NULL,
 CONSTRAINT [PK_tblCartShippingPermission] PRIMARY KEY CLUSTERED 
(
	[nCartShippingPermissionKey] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END

