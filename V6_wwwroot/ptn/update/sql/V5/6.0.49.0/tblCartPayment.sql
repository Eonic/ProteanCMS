CREATE TABLE [dbo].[tblCartPayment](
	[nCartPaymentKey] [int] IDENTITY(1,1) NOT NULL,
	[nCartOrderId] [int] NOT NULL,
	[nCartPaymentMethodId] [int] NULL,
	[nPaymentAmount] [money] NOT NULL,
	[bFull] [bit] NULL,	
	[bPart] [bit] NULL,
	[bRefund] [bit] NULL,
	[bSettlement] [bit] NULL,
	[nAuditId] [int] NULL
) ON [PRIMARY]

