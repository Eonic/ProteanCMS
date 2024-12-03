CREATE PROCEDURE [dbo].[spGetContentIdFromOrderReference]
	-- Add the parameters for the stored procedure here
	@orderRef INT,
	@ProductName NVARCHAR(200)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select top 1 tblCartItem.nItemId  from tblCartOrder 
	inner join tblCartItem on tblCartOrder.nCartOrderKey= nCartOrderID 
	where tblCartOrder.cCartForiegnRef = @orderRef and tblCartItem.cItemName like '%'+@ProductName+'%' 
	and tblCartItem.nParentId =0

END
