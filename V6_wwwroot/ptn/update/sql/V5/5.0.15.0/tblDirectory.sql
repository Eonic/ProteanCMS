
if not Exists(select * from sys.columns where Name = N'cDirEmail' and Object_ID = Object_ID(N'tblDirectory')) 
BEGIN
ALTER TABLE dbo.tblDirectory ADD
	cDirEmail nvarchar(255) NULL
END