/*

   06 June 2008 10:05:03

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
CREATE TABLE dbo.Tmp_tblActivityLog
	(
	nActivityKey int NOT NULL IDENTITY (1, 1),
	nUserDirId int NULL,
	nStructId int NULL,
	nArtId int NULL,
	nOtherId int NULL,
	dDateTime datetime NOT NULL,
	nActivityType int NOT NULL,
	cActivityDetail nvarchar(800) NULL,
	cSessionId nvarchar(50) NOT NULL
	)  ON [PRIMARY]

SET IDENTITY_INSERT dbo.Tmp_tblActivityLog ON

IF EXISTS(SELECT * FROM dbo.tblActivityLog)
	 EXEC('INSERT INTO dbo.Tmp_tblActivityLog (nActivityKey, nUserDirId, nStructId, nArtId, dDateTime, nActivityType, cActivityDetail, cSessionId)
		SELECT nActivityKey, nUserDirId, nStructId, nArtId, dDateTime, nActivityType, cActivityDetail, cSessionId FROM dbo.tblActivityLog TABLOCKX')

SET IDENTITY_INSERT dbo.Tmp_tblActivityLog OFF

DROP TABLE dbo.tblActivityLog

EXECUTE sp_rename N'dbo.Tmp_tblActivityLog', N'tblActivityLog', 'OBJECT'

ALTER TABLE dbo.tblActivityLog ADD CONSTRAINT
	PK_tblActivityLog PRIMARY KEY CLUSTERED 
	(
	nActivityKey
	) WITH FILLFACTOR = 90 ON [PRIMARY]

COMMIT
