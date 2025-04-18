/*
   28 August 202410:01:04
   User: ewadmin
   Server: sql01.eonichost.co.uk
   Database: ew_goodnewsuk_com
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
GO
ALTER TABLE dbo.tblCartContact ADD
	nLat nvarchar(50) NULL,
	nLong nvarchar(50) NULL,
	cContactAddress2 nvarchar(255) NULL
GO
ALTER TABLE dbo.tblCartContact SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
