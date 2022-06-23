DECLARE @DatabaseName as varchar(100)
DECLARE @SQL as varchar(100)
DECLARE @CompatibilityLevel As Int
SELECT @DatabaseName=DB_NAME() 
SELECT  @CompatibilityLevel=compatibility_level FROM sys.databases WHERE name = @DatabaseName;  

IF (@CompatibilityLevel<130)
BEGIN
	SET @SQL='ALTER DATABASE ' + @DatabaseName +' SET COMPATIBILITY_LEVEL = 130'
	Exec(@SQL)
END

