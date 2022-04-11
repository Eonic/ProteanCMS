--Brings two values out of the Discount XML and adds them as top-level fields to aid performance
if not Exists(select * from sys.columns where Name = N'cContactForiegnRef' and Object_ID = Object_ID(N'tblCartContact')) 
BEGIN
ALTER TABLE dbo.tblCartContact ADD
	cContactForiegnRef nvarchar(50) NULL
END