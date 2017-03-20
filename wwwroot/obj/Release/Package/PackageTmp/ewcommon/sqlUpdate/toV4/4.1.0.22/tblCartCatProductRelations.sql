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
CREATE TABLE dbo.Tmp_tblCartCatProductRelations
	(
	nCatProductRelKey int NOT NULL IDENTITY (1, 1),
	nContentId int NOT NULL,
	nCatId int NOT NULL,
	nDisplayOrder int NULL,
	nAuditId int NULL
	)  ON [PRIMARY]

SET IDENTITY_INSERT dbo.Tmp_tblCartCatProductRelations ON

IF EXISTS(SELECT * FROM dbo.tblCartCatProductRelations)
	 EXEC('INSERT INTO dbo.Tmp_tblCartCatProductRelations (nCatProductRelKey, nContentId, nCatId, nAuditId)
		SELECT nCatProductRelKey, nContentId, nCatId, nAuditId FROM dbo.tblCartCatProductRelations TABLOCKX')

SET IDENTITY_INSERT dbo.Tmp_tblCartCatProductRelations OFF

DROP TABLE dbo.tblCartCatProductRelations

EXECUTE sp_rename N'dbo.Tmp_tblCartCatProductRelations', N'tblCartCatProductRelations', 'OBJECT'

ALTER TABLE dbo.tblCartCatProductRelations ADD CONSTRAINT
	PK_tblCartCatProductRelations PRIMARY KEY CLUSTERED 
	(
	nCatProductRelKey
	) WITH FILLFACTOR = 90 ON [PRIMARY]


COMMIT
