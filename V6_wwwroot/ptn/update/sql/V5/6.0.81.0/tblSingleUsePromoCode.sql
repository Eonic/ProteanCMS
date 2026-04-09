
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tblSingleUsePromoCode' AND xtype='U')
BEGIN
	CREATE TABLE [dbo].[tblSingleUsePromoCode](
		[nSinglePromoId] [int] IDENTITY(1,1) NOT NULL,
		[OrderId] [int] NOT NULL,
		[PromoCode] [nvarchar](200) NOT NULL
	 CONSTRAINT [PK_tblSingleUsePromoCode] PRIMARY KEY CLUSTERED 
	(
		[nSinglePromoId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END