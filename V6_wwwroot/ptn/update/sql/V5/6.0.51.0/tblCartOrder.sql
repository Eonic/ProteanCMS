if not Exists(select * from sys.columns where Name = N'nAmountReceived' and Object_ID = Object_ID(N'tblCartOrder')) 
BEGIN
ALTER TABLE dbo.tblCartOrder ADD
	nAmountReceived money NULL
END

