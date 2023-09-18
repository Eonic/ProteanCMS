
--To get distinct events and tickets
CREATE VIEW dbo.vw_GetDistinctEventsTickets AS 
SELECT vw_GetDistinctEvents.nContentKey As 'EventKey', vw_GetDistinctEvents.nContentParentId, vw_GetDistinctEvents.cContentName AS 'EventName', vw_GetDistinctEvents.cContentSchemaName AS 'TypeE',
vw_GetDistinctTickets.nContentKey AS 'TicketId', vw_GetDistinctTickets.cContentName AS 'TicketName', vw_GetDistinctTickets.cContentSchemaName AS 'TypeT'
FROM vw_GetDistinctEvents INNER JOIN vw_GetDistinctTickets ON vw_GetDistinctEvents.nContentParentId = vw_GetDistinctTickets.nContentChildId
