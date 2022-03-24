CREATE PROCEDURE [dbo].[spGetEventsTicketsCartsPrice]     
AS  
  
BEGIN    
 SET NOCOUNT ON;    
  
--To join tickets with cart and getting price  
SELECT vw_GetEventsTicketWithCartContact.*, CAST(ROUND((vw_GetEventsTicketWithCartContact.Price * vw_GetEventsTicketWithCartContact.Quantity), 2) AS DECIMAL(16,2)) AS 'LineTotalPrice', 
		tblCartOrder.nTaxRate AS 'Booking%', 
		(Quantity * CAST(ROUND(tblCartOrder.nTaxRate / 100 * vw_GetEventsTicketWithCartContact.Price, 2) AS DECIMAL(16,2))) AS 'BookingFee'
FROM 
	vw_GetEventsTicketWithCartContact 
	INNER JOIN tblCartOrder ON vw_GetEventsTicketWithCartContact.CartId = tblCartOrder.nCartOrderKey
	INNER JOIN tblCartPayment ON vw_GetEventsTicketWithCartContact.CartId = tblCartPayment.nCartOrderId 
  
END 

