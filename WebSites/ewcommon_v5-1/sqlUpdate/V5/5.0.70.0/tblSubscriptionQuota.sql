
if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblSubscriptionQuota]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN

CREATE TABLE [dbo].[tblSubscriptionQuota](
	[nSubQuotaKey] [int] IDENTITY(1,1) NOT NULL,
	[nSubId] [int] NULL,
	[cLimitName] [nvarchar](255) NULL,
	[cLimitType] [nvarchar](50) NULL,
	[cLimitCount] [float] NULL,
	[nDirIdAllowed] [int] NULL,
	[nAdditionalCharge] [float] NULL,
	[nAuditId] [int] NULL,
 CONSTRAINT [PK_tblSubscriptionLimit] PRIMARY KEY CLUSTERED 
(
	[nSubQuotaKey] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

END

