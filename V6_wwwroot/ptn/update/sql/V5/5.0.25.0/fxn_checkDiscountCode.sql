
CREATE FUNCTION [dbo].[fxn_checkDiscountCode] (@nDiscountId int, @cCode nvarchar(255))  
RETURNS int

AS  
BEGIN
	DECLARE @returnVal int 
	SET @returnVal = (
		Select c.nCodeKey from tblCodes c
		inner join tblCodes cg on cg.nCodeKey = c.nCodeParentid
		inner join tblAudit cga on cg.nAuditId = cga.nAuditKey
		inner join tblCartDiscountRules dr on dr.nDiscountCodeBank = cg.nCodeKey
		Where dr.nDiscountKey = @nDiscountId
		AND c.cCode LIKE @cCode
		AND c.dUseDate is null
		AND (cga.dPublishDate is null or cga.dPublishDate >= getdate())
		AND (cga.dExpireDate is null or cga.dPublishDate <= getdate())
		AND (cga.nStatus = 1)
	)
	
	return @returnVal
	
END