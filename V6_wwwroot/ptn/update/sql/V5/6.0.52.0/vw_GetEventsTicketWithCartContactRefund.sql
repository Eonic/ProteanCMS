
--To get list of events/tickets with contact and price. This is for the carts with status 7 (Refunded)
CREATE VIEW dbo.vw_GetEventsTicketWithCartContactRefund AS 
SELECT distinct Items.nCartOrderId AS 'CartId', vw_GetDistinctEventsTickets.EventName, vw_GetDistinctEventsTickets.TicketName, vw_GetDistinctEventsTickets.TicketId,
		dbo.tblCartContact.cContactName AS 'CustomerName', dbo.tblCartContact.cContactEmail AS 'Email', Items.nQuantity AS 'Quantity',
		(SELECT        CASE WHEN SUM(Options.nPrice) IS NULL THEN 0 WHEN SUM(Options.nQuantity) IS NULL THEN 0 ELSE SUM(Options.nPrice * Options.nQuantity) END AS nOptionsPrice  
              FROM            dbo.tblCartItem AS Options  
              WHERE        (nParentId = Items.nCartItemKey)) + Items.nPrice AS 'Price'
FROM 
		vw_GetDistinctEventsTickets
		INNER JOIN dbo.tblCartItem AS Items ON Items.nItemId = dbo.vw_GetDistinctEventsTickets.TicketId 
		INNER JOIN  dbo.tblCartOrder ON Items.nCartOrderId = dbo.tblCartOrder.nCartOrderKey 
		INNER JOIN dbo.tblCartContact ON dbo.tblCartContact.nContactCartId = dbo.tblCartOrder.nCartOrderKey
		INNER JOIN dbo.tblAudit ON dbo.tblCartOrder.nAuditId = dbo.tblAudit.nAuditKey
WHERE
		(Items.nParentId IS NULL OR Items.nParentId = 0) AND (dbo.tblCartContact.cContactType LIKE N'Billing Address')  
		AND nCartStatus IN (7)
