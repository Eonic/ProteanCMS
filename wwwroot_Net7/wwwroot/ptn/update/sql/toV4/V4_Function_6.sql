CREATE FUNCTION dbo.fxn_shippingTotal (@nShipOptId int, @nAmount int, @nQuantity int, @nWeight int)  
RETURNS float
AS  
BEGIN
	
DECLARE @value float
DECLARE @optPercent float
DECLARE @optHandlingFixedCost float
DECLARE @optHandlingPercent float
	set @value = (
	SELECT nShipOptCost  from tblCartShippingMethods opt
		where opt.nShipOptKey = @nShipOptId
	)
	set @optPercent = (
	SELECT nShipOptPercentage  from tblCartShippingMethods opt
		where opt.nShipOptKey = @nShipOptId
	)
	set @optHandlingFixedCost = (
	SELECT nShipOptHandlingFixedCost  from tblCartShippingMethods opt
		where opt.nShipOptKey = @nShipOptId
	)
	set @optHandlingPercent = (
	SELECT nShipOptHandlingPercentage  from tblCartShippingMethods opt
		where opt.nShipOptKey = @nShipOptId
	)
	if @optPercent > 0
	BEGIN
		set @value = @value + ( (@optPercent / 100) * @nAmount )
	END
	if @optHandlingFixedCost > 0
	BEGIN
		set @value = @value + (@optHandlingFixedCost * @nQuantity)
	END
	if @optHandlingPercent > 0
	BEGIN
		set @value = @value + ( (@optHandlingPercent / 100) * @nAmount )
	END
	RETURN(@value)
	
END