--Brings two values out of the Discount XML and adds them as top-level fields to aid performance
if not Exists(select * from sys.columns where Name = N'nOtherId' and Object_ID = Object_ID(N'tblActivityLog')) 
BEGIN
ALTER TABLE dbo.tblActivityLog ADD
	nOtherId integer NULL
END