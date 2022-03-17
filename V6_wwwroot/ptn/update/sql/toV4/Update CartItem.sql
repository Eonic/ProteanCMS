/*

   23 November 2006 16:19:45

   User: 

   Server: EONICPROD01

   Database: ew_v4demo

   Application: MS SQLEM - Data Tools

*/



BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo.tblCartItem
	DROP CONSTRAINT FK_tblCartItem_tblContent
GO
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo.tblCartItem
	DROP CONSTRAINT FK_tblCartItem_tblCartOrder
GO
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo.tblCartItem
	DROP CONSTRAINT FK_tblCartItem_tblAudit
GO
COMMIT
BEGIN TRANSACTION
CREATE TABLE dbo.Tmp_tblCartItem
	(
	nCartItemKey int NOT NULL IDENTITY (1, 1),
	nCartOrderId int NULL,
	nItemId int NULL,
	nParentId int NULL,
	cItemRef nvarchar(50) NULL,
	cItemURL nvarchar(255) NOT NULL,
	cItemName nvarchar(255) NULL,
	nItemOptGrpIdx int NULL,
	nItemOptIdx int NULL,
	nPrice money NULL,
	nShpCat int NULL,
	nDiscountCat int NULL,
	nDiscountValue money NULL,
	nTaxRate decimal(18, 0) NULL,
	nQuantity decimal(18, 0) NULL,
	nWeight float(53) NULL,
	nAuditId int NULL
	)  ON [PRIMARY]
GO
SET IDENTITY_INSERT dbo.Tmp_tblCartItem ON
GO
IF EXISTS(SELECT * FROM dbo.tblCartItem)
	 EXEC('INSERT INTO dbo.Tmp_tblCartItem (nCartItemKey, nCartOrderId, nItemId, cItemRef, cItemURL, cItemName, nItemOptGrpIdx, nItemOptIdx, nPrice, nShpCat, nDiscountCat, nDiscountValue, nTaxRate, nQuantity, nWeight, nAuditId)
		SELECT nCartItemKey, nCartOrderId, nItemId, cItemRef, cItemURL, cItemName, CONVERT(int, cItemOption1), CONVERT(int, cItemOption2), nPrice, nShpCat, nDiscountCat, nDiscountValue, nTaxRate, nQuantity, nWeight, nAuditId FROM dbo.tblCartItem TABLOCKX')
GO
SET IDENTITY_INSERT dbo.Tmp_tblCartItem OFF
GO
DROP TABLE dbo.tblCartItem
GO
EXECUTE sp_rename N'dbo.Tmp_tblCartItem', N'tblCartItem', 'OBJECT'
GO
ALTER TABLE dbo.tblCartItem ADD CONSTRAINT
	PK_tblCartItem PRIMARY KEY CLUSTERED 
	(
	nCartItemKey
	) WITH FILLFACTOR = 90 ON [PRIMARY]

GO
ALTER TABLE dbo.tblCartItem WITH NOCHECK ADD CONSTRAINT
	FK_tblCartItem_tblAudit FOREIGN KEY
	(
	nAuditId
	) REFERENCES dbo.tblAudit
	(
	nAuditKey
	)
GO
ALTER TABLE dbo.tblCartItem WITH NOCHECK ADD CONSTRAINT
	FK_tblCartItem_tblCartOrder FOREIGN KEY
	(
	nCartOrderId
	) REFERENCES dbo.tblCartOrder
	(
	nCartOrderKey
	)
GO
ALTER TABLE dbo.tblCartItem WITH NOCHECK ADD CONSTRAINT
	FK_tblCartItem_tblContent FOREIGN KEY
	(
	nItemId
	) REFERENCES dbo.tblContent
	(
	nContentKey
	)
GO
COMMIT
