if not Exists(select * from sys.columns where Name = N'nLastPaymentMade' and Object_ID = Object_ID(N'tblCartOrder')) 
BEGIN
ALTER TABLE dbo.tblCartOrder ADD
	nLastPaymentMade money NULL
END

