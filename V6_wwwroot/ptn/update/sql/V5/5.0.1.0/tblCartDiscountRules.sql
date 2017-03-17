--Brings two values out of the Discount XML and adds them as top-level fields to aid performance
if not Exists(select * from sys.columns where Name = N'nDiscountCodeType' and Object_ID = Object_ID(N'tblCartDiscountRules')) 
BEGIN
ALTER TABLE dbo.tblCartDiscountRules ADD
	cDiscountUserCode nvarchar(50) NULL,
	nDiscountCodeType int NULL
END



