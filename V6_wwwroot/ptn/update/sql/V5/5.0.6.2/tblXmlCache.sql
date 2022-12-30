--Brings two values out of the Discount XML and adds them as top-level fields to aid performance
if not Exists(select * from sys.columns where Name = N'cCacheType' and Object_ID = Object_ID(N'tblXMLCache')) 
BEGIN

ALTER TABLE dbo.tblXMLCache ADD	cCacheType nvarchar(255) NULL
ALTER TABLE dbo.tblXMLCache	ALTER COLUMN cCacheStructure xml NOT NULL

END