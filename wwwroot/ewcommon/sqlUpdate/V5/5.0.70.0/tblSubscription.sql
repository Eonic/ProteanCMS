
if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblSubscription]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN

CREATE TABLE [dbo].[tblSubscription](
	[nSubKey] [int] IDENTITY(1,1) NOT NULL,
	[cForeignRef] [nvarchar](50) NULL,
	[nSubContentId] [int] NULL,
	[nDirId] [int] NULL,
	[nDirType] [int] NULL,
	[cSubName] [nvarchar](255) NULL,
	[cSubXml] [text] NULL,
	[dStartDate] [datetime] NULL,
	[nPeriod] [int] NULL,
	[cPeriodUnit] [nchar](10) NULL,
	[nMinimumTerm] [int] NULL,
	[nRenewalTerm] [int] NULL,
	[nValueNet] [float] NULL,
	[nPaymentMethodId] [int] NULL,
	[bPaymentMethodActive] [bit] NULL,
	[cRenewalStatus] [nvarchar](50) NULL,
	[nAuditId] [int] NULL,
 CONSTRAINT [PK_tblSubscription] PRIMARY KEY CLUSTERED 
(
	[nSubKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
