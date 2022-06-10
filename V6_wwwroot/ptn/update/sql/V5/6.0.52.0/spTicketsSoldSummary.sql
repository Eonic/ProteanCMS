CREATE PROCEDURE [spTicketsSoldSummary]         
AS      
      
BEGIN        
 SET NOCOUNT ON;        
      
--To get sum of booking fees received    
SELECT vw_GetEventsTicketWithCartContact.EventName, vw_GetEventsTicketWithCartContact.TicketName, SUM(vw_GetEventsTicketWithCartContact.Quantity) AS 'TotalTicketsSold',    
  CAST(ROUND(SUM(Quantity * Price), 2) AS DECIMAL(16,2)) AS 'TotalSoldPrice',    
  CAST(ROUND(SUM((Quantity * (tblCartOrder.nTaxRate / 100 * vw_GetEventsTicketWithCartContact.Price))), 2) AS DECIMAL(16,2)) AS 'TotalBookingFee'    
FROM     
 vw_GetEventsTicketWithCartContact     
 INNER JOIN tblCartOrder ON vw_GetEventsTicketWithCartContact.CartId = tblCartOrder.nCartOrderKey    
 --INNER JOIN tblCartPayment ON vw_GetEventsTicketWithCartContact.CartId = tblCartPayment.nCartOrderId    
GROUP BY vw_GetEventsTicketWithCartContact.EventName, vw_GetEventsTicketWithCartContact.TicketName    
      
END     