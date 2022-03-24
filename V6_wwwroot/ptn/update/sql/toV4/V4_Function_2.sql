
CREATE FUNCTION dbo.fxn_addAudit(@dExpireDate datetime)
RETURNS int  
BEGIN 
DECLARE @aid int
EXEC spInsertAudit @d = @dExpireDate, @id = @aid OUTPUT
RETURN @aid
END