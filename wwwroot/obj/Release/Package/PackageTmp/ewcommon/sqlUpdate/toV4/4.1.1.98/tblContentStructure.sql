if not Exists(select * from sys.columns where Name = N'nVersionParId' and Object_ID = Object_ID(N'tblContentStructure')) 
BEGIN
	ALTER TABLE  dbo.tblContentStructure ADD 
	nVersionParId int NULL,
	cVersionLang nvarchar(50) NULL,
	cVersionDescription nvarchar(255) NULL,
	nVersionType nchar(10) NULL
END