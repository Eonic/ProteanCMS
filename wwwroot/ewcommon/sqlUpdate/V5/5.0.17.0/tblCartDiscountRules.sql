if not Exists(select * from sys.columns where Name = N'nDiscountCodeBank' and Object_ID = Object_ID(N'tblCartDiscountRules')) 
BEGIN
ALTER TABLE dbo.tblCartDiscountRules ADD
	nDiscountCodeBank int NULL
END
