

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
CREATE TABLE dbo.Tmp_tblCartPaymentMethod
	(
	nPayMthdKey int NOT NULL IDENTITY (1, 1),
	nPayMthdUserId int NULL,
	cPayMthdProviderName nvarchar(50) NULL,
	cPayMthdProviderRef nvarchar(50) NULL,
	cPayMthdCardType nvarchar(50) NULL,
	cPayMthdDescription nvarchar(255) NULL,
	cPayMthdAcctName nvarchar(255) NULL,
	cPayMthdDetailXml ntext NULL,
	nAuditId int NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

SET IDENTITY_INSERT dbo.Tmp_tblCartPaymentMethod ON

IF EXISTS(SELECT * FROM dbo.tblCartPaymentMethod)
	 EXEC('INSERT INTO dbo.Tmp_tblCartPaymentMethod (nPayMthdKey, nPayMthdUserId, cPayMthdProviderName, cPayMthdProviderRef, cPayMthdCardType, cPayMthdDescription, cPayMthdAcctName, cPayMthdDetailXml, nAuditId)
		SELECT nPayMthdKey, nPayMthdUserId, cPayMthdProviderName, cPayMthdProviderRef, cPayMthdCardType, cPayMthdDescription, cPayMthdAcctName, CONVERT(ntext, cPayMthdDetailXml), nAuditId FROM dbo.tblCartPaymentMethod TABLOCKX')

SET IDENTITY_INSERT dbo.Tmp_tblCartPaymentMethod OFF

DROP TABLE dbo.tblCartPaymentMethod

EXECUTE sp_rename N'dbo.Tmp_tblCartPaymentMethod', N'tblCartPaymentMethod', 'OBJECT'

ALTER TABLE dbo.tblCartPaymentMethod ADD CONSTRAINT
	PK_tblCartPaymentMethod PRIMARY KEY CLUSTERED 
	(
	nPayMthdKey
	) WITH FILLFACTOR = 90 ON [PRIMARY]


COMMIT
