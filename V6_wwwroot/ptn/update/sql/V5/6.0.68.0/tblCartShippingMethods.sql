if not Exists(select * from sys.columns where Name = N'bOverrideForWholeOrder' and Object_ID = Object_ID(N'tblCartShippingMethods')) 
BEGIN
ALTER TABLE dbo.tblCartShippingMethods ADD
	bOverrideForWholeOrder bit NULL
END

