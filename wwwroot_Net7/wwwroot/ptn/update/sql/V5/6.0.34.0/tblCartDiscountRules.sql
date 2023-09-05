if not Exists(select * from sys.columns where Name = N'bAllProductExcludeGroups' and Object_ID = Object_ID(N'tblCartDiscountRules')) 
BEGIN
ALTER TABLE dbo.tblCartDiscountRules ADD
	bAllProductExcludeGroups bit NULL
END

