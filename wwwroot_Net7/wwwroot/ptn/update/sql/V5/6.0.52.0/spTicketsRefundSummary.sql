CREATE PROCEDURE [dbo].[spTicketsRefundSummary]         
AS      
      
BEGIN        
 SET NOCOUNT ON;        
      
--To get sum of booking fees refunded    
SELECT vw_GetEventsTicketWithCartContactRefund.EventName, vw_GetEventsTicketWithCartContactRefund.TicketName, SUM(vw_GetEventsTicketWithCartContactRefund.Quantity) AS 'TotalTicketsRefunded',    
  CAST(ROUND(SUM(Quantity * Price), 2) AS DECIMAL(16,2)) AS 'TotalRefundedPrice',    
  CAST(ROUND(SUM((Quantity * (tblCartOrder.nTaxRate / 100 * vw_GetEventsTicketWithCartContactRefund.Price))), 2) AS DECIMAL(16,2)) AS 'TotalBookingFeeRefunded'    
FROM     
 vw_GetEventsTicketWithCartContactRefund     
 INNER JOIN tblCartOrder ON vw_GetEventsTicketWithCartContactRefund.CartId = tblCartOrder.nCartOrderKey    
 --INNER JOIN tblCartPayment ON vw_GetEventsTicketWithCartContactRefund.CartId = tblCartPayment.nCartOrderId    
GROUP BY vw_GetEventsTicketWithCartContactRefund.EventName, vw_GetEventsTicketWithCartContactRefund.TicketName    
      
END 