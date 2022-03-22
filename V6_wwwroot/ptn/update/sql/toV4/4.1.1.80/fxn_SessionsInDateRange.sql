

CREATE   FUNCTION [dbo].[fxn_SessionsInDateRange] (@FROM datetime,@TO datetime)
RETURNS  @Sessions TABLE (SessionID nvarchar(255))
AS  
BEGIN

	INSERT INTO @Sessions
SELECT     cSessionId
FROM         tblActivityLog
WHERE ((SELECT     TOP 1 dDateTime AS Expr1
                              FROM         tblActivityLog SessionStart
                              WHERE     (cSessionId = tblActivityLog.cSessionId)
                              ORDER BY dDateTime)) >=@FROM AND 
((SELECT     TOP 1 dDateTime AS Expr1
                              FROM         tblActivityLog SessionStart
                              WHERE     (cSessionId = tblActivityLog.cSessionId)
                              ORDER BY dDateTime)) <=@TO
GROUP BY cSessionId
	RETURN
END


