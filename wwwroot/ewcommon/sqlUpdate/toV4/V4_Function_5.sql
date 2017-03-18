CREATE FUNCTION dbo.fxn_getUserCompanies
(@nDirId int)
RETURNS nvarchar(128)  
BEGIN 

DECLARE @companylist varchar(255), @company varchar(255)
DECLARE companies CURSOR FOR 
	SELECT  DISTINCT dircompanies.cDirName
	FROM	tblDirectoryRelation user2company
			INNER JOIN tblDirectory dircompanies 
				ON user2company.nDirChildId = @nDirId
					AND user2company.nDirParentId = dircompanies.nDirKey 
					AND dircompanies.cDirSchema = 'Company'
	ORDER BY dircompanies.cDirName

	SET @companylist = ''

	OPEN companies 

	FETCH NEXT FROM companies INTO @company 

	WHILE @@FETCH_STATUS = 0
	BEGIN
	
	   	SET @companylist = @companylist + @company
		FETCH NEXT FROM companies INTO @company 
		IF @@FETCH_STATUS = 0
		BEGIN
			SET @companylist = @companylist + ', '
		END
	END

	CLOSE companies 
	DEALLOCATE companies 

	RETURN @companylist 
END