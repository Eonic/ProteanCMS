ALTER PROCEDURE [dbo].[spCartActivityLowLevel]
	-- Add the parameters for the stored procedure here
	 @dBegin datetime,
 @dEnd datetime,
 @nProductId int,
 @cCurrencySymbol nvarchar(50) = '',
 @nOrderStatus nvarchar(50) = "6,9,17",
 @cOrderType nvarchar(50)='ORDER'
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;



	SELECT     
	vw_CartOverView.nCartOrderKey AS [Order_Id],
	vw_CartOverView.cContentSchemaName AS [Item_Type],
	vw_CartOverView.nContentKey AS [Item_Id],
    vw_CartOverView.cContentName AS [Item_Name],
	vw_CartOverView.nQuantity AS Quantity,
	(vw_CartOverView.nQuantity * vw_CartOverView.nLinePrice) AS [Total_Cost],

vw_CartOverView.cCurrency AS Currency_Symbol, 
tblCartOrder.cCartXml
FROM         vw_CartOverView INNER JOIN
                      tblCartOrder ON vw_CartOverView.nCartOrderKey = tblCartOrder.nCartOrderKey

WHERE     
	(vw_CartOverView.dCartDate >= @dBegin) 
	AND (vw_CartOverView.dCartDate <= @dEnd)
	AND (vw_CartOverView.nCartStatus IN((SELECT convert(int, value) FROM string_split(@nOrderStatus, ',')))
	AND (vw_CartOverView.cCartSchemaName = @cOrderType)
	AND (
		@cCurrencySymbol = ''
		OR 
		vw_CartOverView.cCurrency LIKE  @cCurrencySymbol 
		)
	AND (vw_CartOverView.nContentKey = @nProductId)
 )
ORDER BY vw_CartOverView.cContentName

END
