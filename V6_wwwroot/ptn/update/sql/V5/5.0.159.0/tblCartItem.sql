if not Exists(select * from sys.columns where Name = N'xItemXml' and Object_ID = Object_ID(N'tblCartItem')) 
BEGIN
ALTER TABLE dbo.tblCartItem ADD
	xItemXml xml NULL
END
