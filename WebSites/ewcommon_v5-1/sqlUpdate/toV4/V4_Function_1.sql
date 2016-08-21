CREATE FUNCTION [dbo].[fxn_SearchXML] (@sXml ntext, @sXPath ntext)  
RETURNS int
AS  
BEGIN
	
	DECLARE @object int
	DECLARE @hr int
	DECLARE @value int
	DECLARE @src varchar(255), @desc varchar(255)
	
	EXEC @hr = sp_OACreate 'EonicDbTools.DbXML', @object OUT
	EXEC @hr = sp_OAMethod @object, 'QueryXmlString', @value OUT , @sXml, @sXPath
	EXEC @hr = sp_OADestroy @object
	IF @hr <> 0
	BEGIN
		EXEC sp_OAGetErrorInfo @object
	END
	RETURN @value
	
END