/*
   01 April 201811:02:50
   User: ewadmin
   Server: sql02.eonichost.co.uk
   Database: ew_demo_storeandinsure_co_uk
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
if not Exists(select * from sys.columns where Name = N'xNotesXml' and Object_ID = Object_ID(N'tblSubscriptionRenewal')) 
BEGIN
	CREATE TABLE dbo.Tmp_tblSubscriptionRenewal
		(
		nSubRenewalKey int NOT NULL IDENTITY (1, 1),
		nSubId int NULL,
		nOrderId int NULL,
		nPaymentMethodId int NULL,
		nPaymentStatus int NULL,
		xNotesXml xml NULL,
		nAuditId nchar(10) NULL
		)  ON [PRIMARY]
		 TEXTIMAGE_ON [PRIMARY]

	ALTER TABLE dbo.Tmp_tblSubscriptionRenewal SET (LOCK_ESCALATION = TABLE)

	SET IDENTITY_INSERT dbo.Tmp_tblSubscriptionRenewal ON

	IF EXISTS(SELECT * FROM dbo.tblSubscriptionRenewal)
		 EXEC('INSERT INTO dbo.Tmp_tblSubscriptionRenewal (nSubRenewalKey, nSubId, nPaymentMethodId, nPaymentStatus, xNotesXml, nAuditId)
			SELECT nSubRenewalKey, nSubId, nPaymentMethodId, nPaymentStatus, cNotesXml, nAuditId FROM dbo.tblSubscriptionRenewal WITH (HOLDLOCK TABLOCKX)')

	SET IDENTITY_INSERT dbo.Tmp_tblSubscriptionRenewal OFF

	DROP TABLE dbo.tblSubscriptionRenewal

	EXECUTE sp_rename N'dbo.Tmp_tblSubscriptionRenewal', N'tblSubscriptionRenewal', 'OBJECT' 

	ALTER TABLE dbo.tblSubscriptionRenewal ADD CONSTRAINT
		PK_tblSubscriptionRenewal PRIMARY KEY CLUSTERED 
		(
		nSubRenewalKey
		) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


END
