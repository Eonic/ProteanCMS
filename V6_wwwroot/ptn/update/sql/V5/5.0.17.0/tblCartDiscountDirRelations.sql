if not Exists(select * from sys.columns where Name = N'nPermLevel' and Object_ID = Object_ID(N'tblCartDiscountDirRelations')) 
BEGIN
ALTER TABLE dbo.tblCartDiscountDirRelations ADD
	nPermLevel integer NULL
END
