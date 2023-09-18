if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblSubscriptionRenewal]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN

CREATE TABLE [dbo].[tblSubscriptionRenewal](
	[nSubRenewalKey] [int] NOT NULL,
	[nSubId] [int] NULL,
	[nPaymentMethodId] [int] NULL,
	[nPaymentStatus] [int] NULL,
	[cNotesXml] [text] NULL,
	[nAuditId] [nchar](10) NULL,
 CONSTRAINT [PK_tblSubscriptionRenewal] PRIMARY KEY CLUSTERED 
(
	[nSubRenewalKey] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

END

