
if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartCarrier]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN

CREATE TABLE [dbo].[tblCartCarrier](
	[nCarrierKey] [int] IDENTITY(1,1) NOT NULL,
	[cCarrierName] [nvarchar](50) NULL,
	[cCarrierTrackingInstructions] [text] NULL,
	[nAuditId] [int] NULL,
 CONSTRAINT [PK_tblCartCarrier] PRIMARY KEY CLUSTERED 
(
	[nCarrierKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

END

