if not Exists(select * from sys.columns where Name = N'nOtherId' and Object_ID = Object_ID(N'tblActivityLog')) 
BEGIN
	ALTER TABLE tblActivityLog ADD nOtherId int NULL
END
