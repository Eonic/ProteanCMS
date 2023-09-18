--Brings two values out of the Discount XML and adds them as top-level fields to aid performance
if not Exists(select * from sys.columns where Name = N'cIPAddress' and Object_ID = Object_ID(N'tblActivityLog')) 
BEGIN
ALTER TABLE dbo.tblActivityLog ADD
	cIPAddress nvarchar(15) NULL
END