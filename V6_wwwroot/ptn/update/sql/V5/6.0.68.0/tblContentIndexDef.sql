if not Exists(select * from sys.columns where Name = N'bProductRefForSKU' and Object_ID = Object_ID(N'tblContentIndexDef')) 
BEGIN
ALTER TABLE dbo.tblContentIndexDef ADD
	bProductRefForSKU int NULL
END

