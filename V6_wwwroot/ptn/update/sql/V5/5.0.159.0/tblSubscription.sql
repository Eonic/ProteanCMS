if not Exists(select * from sys.columns where Name = N'nOrderId' and Object_ID = Object_ID(N'tblSubscription')) 
BEGIN
ALTER TABLE dbo.tblSubscription ADD
	nOrderId int NULL
END
