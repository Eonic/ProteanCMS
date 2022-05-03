if not Exists(select * from sys.columns where Name = N'cRelationType' and Object_ID = Object_ID(N'tblContentRelation')) 
BEGIN
	ALTER TABLE tblContentRelation ADD cRelationType nvarchar(50) NULL
END