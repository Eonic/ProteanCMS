ALTER PROCEDURE [dbo].[spCartActivityMedLevel] 
@dBegin datetime,
@dEnd datetime, 
@cProductType nvarchar(50), 
@cCurrencySymbol nvarchar(50) = '',
@nOrderStatus nvarchar(50) = '6,9,17',
@cOrderType nvarchar(50) = 'ORDER'
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT 
	cContentSchemaName AS Item_Type,
	nContentKey AS Item_Id,
	cContentName AS Item_Name,
	SUM(nQuantity) AS Quantity,
	SUM(nQuantity * nLinePrice) AS Total_Cost,
	cCurrency AS Currency_Symbol
FROM         vw_CartOverView

WHERE     
	(dCartDate >= @dBegin) 
	AND (dCartDate <= @dEnd)
	AND (nCartStatus  IN((SELECT convert(int, value) FROM string_split(@nOrderStatus, ',')))
	AND (cCartSchemaName = @cOrderType)
	AND (
		cCurrency = CASE WHEN @cCurrencySymbol = '' THEN '' ELSE  @cCurrencySymbol END
		OR 
		cCurrency = CASE WHEN @cCurrencySymbol = '' THEN NULL ELSE  @cCurrencySymbol END
		)
	AND (cContentSchemaName = @cProductType)
 )
GROUP BY 
	cCurrency,
	cContentSchemaName,
	nContentKey,
	cContentName
ORDER BY cContentName

END
