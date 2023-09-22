ALTER PROCEDURE [dbo].[spPurchasersLast2Years]
AS
BEGIN
	SELECT cc.* 
	FROM tblCartOrder co
	inner Join tblAudit a on co.nAuditId = a.nAuditKey
	inner Join (SELECT *, ROW_NUMBER() OVER (PARTITION BY cContactEmail ORDER BY cContactEmail DESC) AS RowNumber
         FROM  tblCartContact) cc on co.nCartOrderKey = cc.nContactCartId and cContactType = 'Billing Address'
	where co.nCartStatus > 4
	and not cc.cContactEmail in (select * from tblOptOutAddresses)
	and a.dUpdateDate > DATEADD(year,-2,GETDATE())
	and cc.RowNumber = 1
	order by cc.cContactEmail
END