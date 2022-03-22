if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartOrderDelivery]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN

CREATE TABLE [dbo].[tblCartOrderDelivery](
	[nDeliveryKey] [int] IDENTITY(1,1) NOT NULL,
	[nOrderId] [int] NULL,
	[nCarrierId] [int] NULL,
	[cCarrierName] [nvarchar](255) NULL,
	[cCarrierRef] [nvarchar](255) NULL,
	[cCarrierNotes] [text] NULL,
	[dExpectedDeliveryDate] [datetime] NULL,
	[dCollectionDate] [datetime] NULL,
	[nAuditId] [int] NULL,
 CONSTRAINT [PK_tblCartOrderDelivery] PRIMARY KEY CLUSTERED 
(
	[nDeliveryKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

End

