CREATE PROCEDURE [dbo].[spPurchasersLast2Years]
AS
BEGIN
	SELECT cc.* 
	FROM tblCartOrder co
	inner Join tblAudit a on co.nAuditId = a.nAuditKey
	inner Join (SELECT *, ROW_NUMBER() OVER (PARTITION BY cContactEmail ORDER BY tblCartContact.nContactKey DESC) AS RowNumber
         FROM  tblCartContact) cc on co.nCartOrderKey = cc.nContactCartId and cContactType = 'Billing Address'
	where co.nCartStatus > 4
	and cc.RowNumber = 1
	and not cc.cContactEmail in (select * from tblOptOutAddresses)
	and a.dUpdateDate > DATEADD(year,-2,GETDATE())
	order by cc.cContactEmail
END

