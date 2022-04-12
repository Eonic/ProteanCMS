/*

   03 June 2008 10:48:00

   User: 

   Server: EONICPROD01

   Database: ew_durlings

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
CREATE TABLE dbo.tblAlerts
	(
	nAlertKey int NOT NULL IDENTITY (1, 1),
	cAlertTitle nvarchar(50) NULL,
	nDirId int NOT NULL,
	nPageId int NOT NULL,
	nFrequency int NOT NULL,
	cContentType nvarchar(50) NULL,
	cXsltFile nvarchar(50) NULL,
	bUpdatedOnly bit NOT NULL,
	bItterateDown bit NOT NULL,
	bRelatedContentUpdates bit NOT NULL,
	cExtraXml ntext NULL,
	nAuditId int NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.tblAlerts ADD CONSTRAINT
	DF_tblAlerts_nDirId DEFAULT 0 FOR nDirId

ALTER TABLE dbo.tblAlerts ADD CONSTRAINT
	DF_tblAlerts_nPageId DEFAULT 0 FOR nPageId

ALTER TABLE dbo.tblAlerts ADD CONSTRAINT
	DF_tblAlerts_nFrequency DEFAULT 0 FOR nFrequency

ALTER TABLE dbo.tblAlerts ADD CONSTRAINT
	DF_tblAlerts_bUpdatedOnly DEFAULT 0 FOR bUpdatedOnly

ALTER TABLE dbo.tblAlerts ADD CONSTRAINT
	DF_tblAlerts_bItterateDown DEFAULT 0 FOR bItterateDown

ALTER TABLE dbo.tblAlerts ADD CONSTRAINT
	DF_tblAlerts_bRelatedContentUpdates DEFAULT 0 FOR bRelatedContentUpdates

ALTER TABLE dbo.tblAlerts ADD CONSTRAINT
	PK_tblAlerts PRIMARY KEY CLUSTERED 
	(
	nAlertKey
	) ON [PRIMARY]


COMMIT
