if not Exists(select * from sys.columns where Name = N'nDepositAmount' and Object_ID = Object_ID(N'tblCartItem')) 
BEGIN
ALTER TABLE dbo.tblCartItem ADD
	nDepositAmount money NULL
END

