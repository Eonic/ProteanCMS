if not Exists(select * from sys.columns where Name = N'cContactForeignRef' and Object_ID = Object_ID(N'tblCartContact')) 
BEGIN
ALTER TABLE dbo.tblCartContact ADD
	cContactForeignRef nvarchar(255) NULL
END

