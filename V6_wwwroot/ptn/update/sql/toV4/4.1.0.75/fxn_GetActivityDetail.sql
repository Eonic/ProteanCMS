CREATE      FUNCTION dbo.fxn_GetActivityDetail (@nActivityType int, @nId int)
RETURNS  nvarchar(255)
AS
BEGIN
DECLARE @Return nvarchar(255)
	--Check What Type it is and set the tables
IF @nActivityType = 5
	BEGIN
		SET @Return = (SELECT TOP 1 cAlertTitle FROM tblAlerts WHERE nAlertKey = @nId)
	END
ELSE IF @nActivityType = 2
	BEGIN
		SET @Return = (SELECT TOP 1 cStructName FROM dbo.tblContentStructure WHERE nStructKey = @nId)
	END
ELSE IF @nActivityType = 6
	BEGIN
		SET @Return = (SELECT TOP 1 cContentName FROM dbo.tblContent WHERE nContentKey = @nId)
	END

ELSE
	BEGIN
		SET @Return = ''
	END

RETURN  @Return
END