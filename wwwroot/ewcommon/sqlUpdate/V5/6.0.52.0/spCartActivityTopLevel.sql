ALTER PROCEDURE [dbo].[spCartActivityTopLevel]
@dBegin datetime,
@dEnd datetime,
@bSplit bit = 0,
@cCurrencySymbol nvarchar(50) = '',
@nOrderStatus nvarchar(50) = '6,9,12',
@cOrderType nvarchar(50) = 'ORDER'
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
SELECT 
	CASE WHEN @bSplit = 0 THEN 'ALL' ELSE cContentSchemaName END AS Item_Type,
	SUM(nQuantity) AS Quantity,
	SUM(nQuantity * nLinePrice) AS Total_Cost,
	cCurrency AS Currency_Symbol
FROM 
	vw_CartOverView 
WHERE     
	(dCartDate >= @dBegin) 
	AND (dCartDate <= @dEnd)
	AND (nCartStatus  IN((SELECT convert(int, value) FROM string_split(@nOrderStatus, ',')))
	AND (cCartSchemaName = @cOrderType)
	AND (
		cCurrency = CASE WHEN @cCurrencySymbol = '' THEN '*' ELSE  @cCurrencySymbol END
		OR 
		cCurrency = CASE WHEN @cCurrencySymbol = '' THEN NULL ELSE  @cCurrencySymbol END
		)
 )
GROUP BY cCurrency, CASE WHEN @bSplit = 0 THEN 'ALL' ELSE cContentSchemaName  END 
ORDER BY CASE WHEN @bSplit = 0 THEN 'ALL' ELSE cContentSchemaName  END --tblContent.cContentSchemaName 
END
