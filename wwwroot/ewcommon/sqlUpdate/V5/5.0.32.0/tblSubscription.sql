
--These do not work the exists logic is wrong they have been readded in 5.0.70.0 with changes


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblSubscription]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN



CREATE TABLE [dbo].[tblSubscription](
	[nSubKey] [int] NOT NULL,
	[cForeignRef] [nvarchar](50) NULL,
	[nDirId] [int] NULL,
	[nDirType] [int] NULL,
	[cSubName] [nvarchar](255) NULL,
	[cSubXml] [text] NULL,
	[dStartDate] [datetime] NULL,
	[nPeriod] [int] NULL,
	[cPeriodType] [nchar](10) NULL,
	[nValueNet] [float] NULL,
	[nPaymentMethod] [int] NULL,
	[nAuditId] [int] NULL,
 CONSTRAINT [PK_tblSubscription] PRIMARY KEY CLUSTERED 
(
	[nSubKey] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

END

