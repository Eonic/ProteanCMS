if not Exists(select * from sys.columns where Name = N'cContactTelCountryCode' and Object_ID = Object_ID(N'tblCartContact')) 
BEGIN
ALTER TABLE dbo.tblCartContact ADD
	cContactTelCountryCode nvarchar(10) NULL
END


