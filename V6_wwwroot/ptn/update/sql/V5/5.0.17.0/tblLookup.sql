if not Exists(select * from sys.columns where Name = N'nLkpParent' and Object_ID = Object_ID(N'tblLookup')) 
BEGIN
ALTER TABLE dbo.tblLookup ADD
	nLkpParent int NULL,
	nDisplayOrder int NULL,
	nAuditId int NULL
END
