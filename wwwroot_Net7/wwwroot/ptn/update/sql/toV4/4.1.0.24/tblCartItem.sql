/*

   14 December 2007 10:27:16

   User: 

   Server: EONICPROD01

   Database: ew_eonic

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
	cItemUnit nvarchar(50) NULL,
	nPrice money NULL,
	nShpCat int NULL,
	nDiscountCat int NULL,
	nDiscountValue money NULL,
	nTaxRate decimal(18, 0) NULL,
	nQuantity decimal(18, 0) NULL,
	nWeight float(53) NULL,
	nAuditId int NULL
	)  ON [PRIMARY]

SET IDENTITY_INSERT dbo.Tmp_tblCartItem ON

IF EXISTS(SELECT * FROM dbo.tblCartItem)
	 EXEC('INSERT INTO dbo.Tmp_tblCartItem (nCartItemKey, nCartOrderId, nItemId, nParentId, cItemRef, cItemURL, cItemName, nItemOptGrpIdx, nItemOptIdx, nPrice, nShpCat, nDiscountCat, nDiscountValue, nTaxRate, nQuantity, nWeight, nAuditId)
		SELECT nCartItemKey, nCartOrderId, nItemId, nParentId, cItemRef, cItemURL, cItemName, nItemOptGrpIdx, nItemOptIdx, nPrice, nShpCat, nDiscountCat, nDiscountValue, nTaxRate, nQuantity, nWeight, nAuditId FROM dbo.tblCartItem TABLOCKX')

SET IDENTITY_INSERT dbo.Tmp_tblCartItem OFF

DROP TABLE dbo.tblCartItem

EXECUTE sp_rename N'dbo.Tmp_tblCartItem', N'tblCartItem', 'OBJECT'

ALTER TABLE dbo.tblCartItem ADD CONSTRAINT
	PK_tblCartItem PRIMARY KEY CLUSTERED 
	(
	nCartItemKey
	) WITH FILLFACTOR = 90 ON [PRIMARY]


COMMIT
