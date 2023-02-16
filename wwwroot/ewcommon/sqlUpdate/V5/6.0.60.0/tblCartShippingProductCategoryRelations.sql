--Add New table for shipping Group relations
IF  NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'tblCartShippingProductCategoryRelations' AND type = 'U')

BEGIN
CREATE TABLE [dbo].[tblCartShippingProductCategoryRelations](
	[nShipProdCatRelKey] [bigint] IDENTITY(1,1) NOT NULL,
	[nCatId] [int] NULL,
	[nShipOptId] [int] NULL,
	[nRuleType] [nvarchar](500) NULL,
	[nAuditId] [int] NULL,
 CONSTRAINT [PK_tblCartShippingProductCategoryRelations] PRIMARY KEY CLUSTERED 
(
	[nShipProdCatRelKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

END


