ALTER FUNCTION [dbo].[fxn_getStatus]
	(
@id as integer,
@dateNow as datetime
	)
RETURNS integer
AS
	BEGIN
		DECLARE @value int 
SET @value = (
			SELECT nStatus from tblAudit a 
				where nAuditKey = @id 
				AND (((dPublishDate is null or dPublishDate = 0 or dPublishDate <= @dateNow )
				AND (dExpireDate is null or dExpireDate = 0 or dExpireDate >= @dateNow ))
				OR nStatus != 1)
			)
if @value is null
BEGIN			
	SET @value = 7
END	
	RETURN @value
END