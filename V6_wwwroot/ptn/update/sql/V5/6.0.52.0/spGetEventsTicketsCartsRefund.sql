ALTER PROCEDURE [dbo].[spGetEventsTicketsCartsRefund]         
AS      
      
BEGIN        
 SET NOCOUNT ON;        
      
SELECT vw_GetEventsTicketWithCartContactRefund.*,     
  CAST(ROUND((vw_GetEventsTicketWithCartContactRefund.Price * vw_GetEventsTicketWithCartContactRefund.Quantity), 2) AS DECIMAL(16,2)) AS 'LineTotalPrice',     
  tblCartOrder.nTaxRate AS 'Booking%',     
  (Quantity * CAST(ROUND(tblCartOrder.nTaxRate / 100 * vw_GetEventsTicketWithCartContactRefund.Price, 2) AS DECIMAL(16,2))) AS 'BookingFeeRefunded'    
FROM     
 vw_GetEventsTicketWithCartContactRefund     
 INNER JOIN tblCartOrder ON vw_GetEventsTicketWithCartContactRefund.CartId = tblCartOrder.nCartOrderKey    
 --INNER JOIN tblCartPayment ON vw_GetEventsTicketWithCartContactRefund.CartId = tblCartPayment.nCartOrderId    
      
END      