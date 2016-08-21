if not Exists(select * from sys.columns where Name = N'nOrderId' and Object_ID = Object_ID(N'tblCodes')) 
BEGIN
ALTER TABLE dbo.tblCodes ADD
	nOrderId int NULL
END