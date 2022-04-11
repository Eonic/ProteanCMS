if not Exists(select * from sys.columns where Name = N'nAlertParent' and Object_ID = Object_ID(N'tblAlerts')) 
BEGIN
ALTER TABLE dbo.tblAlerts ADD
	nAlertParent int NULL
END
