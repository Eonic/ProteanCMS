
--To get distinct events
CREATE VIEW dbo.vw_GetDistinctEvents AS  
SELECT distinct [tblContentRelation].nContentParentId, [dbo].[tblContent].[nContentKey], [tblContent].cContentName,
		[dbo].[tblContent].cContentSchemaName
FROM [dbo].[tblContentRelation] INNER JOIN [dbo].[tblContent] ON [tblContentRelation].nContentParentId = [tblContent].nContentKey
WHERE [dbo].[tblContent].[cContentSchemaName] = 'Event'


--To get distinct tickets
CREATE VIEW dbo.vw_GetDistinctTickets AS  
SELECT distinct [tblContentRelation].nContentParentId, [tblContentRelation].nContentChildId, [dbo].[tblContent].[nContentKey], [tblContent].cContentName,
		[dbo].[tblContent].cContentSchemaName
FROM [dbo].[tblContentRelation] LEFT OUTER JOIN [dbo].[tblContent] ON [tblContentRelation].nContentParentId = [tblContent].nContentKey
WHERE [dbo].[tblContent].[cContentSchemaName] IN ('Ticket')
 
--To get distinct events and tickets
CREATE VIEW dbo.vw_GetDistinctEventsTickets AS 
SELECT vw_GetDistinctEvents.nContentKey As 'EventKey', vw_GetDistinctEvents.nContentParentId, vw_GetDistinctEvents.cContentName AS 'EventName', vw_GetDistinctEvents.cContentSchemaName AS 'TypeE',
vw_GetDistinctTickets.nContentKey AS 'TicketId', vw_GetDistinctTickets.cContentName AS 'TicketName', vw_GetDistinctTickets.cContentSchemaName AS 'TypeT'
FROM vw_GetDistinctEvents INNER JOIN vw_GetDistinctTickets ON vw_GetDistinctEvents.nContentParentId = vw_GetDistinctTickets.nContentChildId


--To get list of events/tickets with contact and price. This is for the carts with status 6, 9 and 17 (complete, shipped and inprogress)
CREATE VIEW dbo.vw_GetEventsTicketWithCartContact AS 
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
		AND nCartStatus IN (6, 9, 17)

