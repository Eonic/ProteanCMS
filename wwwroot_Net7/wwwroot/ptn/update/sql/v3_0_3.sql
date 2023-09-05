BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo.tbl_ewm_structure

DROP CONSTRAINT FK_tbl_ewm_Structure_tbl_ewm_template

COMMIT
BEGIN TRANSACTION
CREATE TABLE dbo.Tmp_tbl_ewm_structure
	(
	nId int NOT NULL IDENTITY (1, 1),
	nParentId int NULL,
	cName nvarchar(255) NULL,
	cDisplayName nvarchar(255) NULL,
	cURL nvarchar(255) NULL,
	nDisplayOrder int NULL,
	cTemplateName nvarchar(255) NULL,
	nTemplate int NULL,
	nStatus int NULL,
	nCacheMode int NULL,
	dPublishDate datetime NULL,
	dExpireDate datetime NULL,
	cInsertUser nvarchar(100) NULL,
	dInsertDate datetime NULL,
	cUpdateUser nvarchar(100) NULL,
	dUpdateDate datetime NULL
	)  ON [PRIMARY]

SET IDENTITY_INSERT dbo.Tmp_tbl_ewm_structure ON

IF EXISTS(SELECT * FROM dbo.tbl_ewm_structure)
	 EXEC('INSERT INTO dbo.Tmp_tbl_ewm_structure (nId, nParentId, cName, cURL, nDisplayOrder, cTemplateName, nTemplate, nStatus, nCacheMode, dPublishDate, dExpireDate, cInsertUser, dInsertDate, cUpdateUser, dUpdateDate)
		SELECT nId, nParentId, cName, cURL, nDisplayOrder, cTemplateName, nTemplate, nStatus, nCacheMode, dPublishDate, dExpireDate, cInsertUser, dInsertDate, cUpdateUser, dUpdateDate FROM dbo.tbl_ewm_structure (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_tbl_ewm_structure OFF

DROP TABLE dbo.tbl_ewm_structure

EXECUTE sp_rename N'dbo.Tmp_tbl_ewm_structure', N'tbl_ewm_structure', 'OBJECT'

ALTER TABLE dbo.tbl_ewm_structure ADD CONSTRAINT
	PK_tbl_ewm_Structure PRIMARY KEY CLUSTERED 
	(
	nId
	) WITH FILLFACTOR = 90 ON [PRIMARY]


ALTER TABLE dbo.tbl_ewm_structure WITH NOCHECK ADD CONSTRAINT
	FK_tbl_ewm_Structure_tbl_ewm_template FOREIGN KEY
	(
	nTemplate
	) REFERENCES dbo.tbl_ewm_template
	(
	nTpltKey
	)

COMMIT