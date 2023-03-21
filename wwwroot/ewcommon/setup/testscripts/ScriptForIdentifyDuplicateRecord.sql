



SELECT A.nAuditKey,cnt.aCount
FROM tblAudit A
OUTER APPLY
(
    SELECT SUM(AuditCount) As aCount
    FROM (

    SELECT COUNT(C.nAuditId) AS AuditCount FROM tblContent C WHERE C.nAuditId = A.nAuditKey     
	UNION ALL SELECT COUNT(CR.nAuditId) AS AuditCount FROM tblContentRelation CR WHERE CR.nAuditId = A.nAuditKey
	UNION ALL SELECT COUNT(cpm.nAuditId) AS AuditCount FROM tblCartPaymentMethod cpm WHERE cpm.nAuditId = A.nAuditKey
	UNION ALL SELECT COUNT(al.nAuditId) AS AuditCount FROM tblAlerts al  WHERE al.nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblCartContact	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblCodes	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblCartDiscountRules	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblContentIndexDef	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblCartShippingPermission	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblCartPayment	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblCartCatProductRelations	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblCartDiscountDirRelations	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblCartDiscountProdCatRelations	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblCartItem	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblCartOrder	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblCartProductCategories	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblCartShippingLocations	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblCartShippingMethods	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblCartShippingRelations	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblContent	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblContentLocation	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblContentRelation	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblITBSupplierOffer	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblContentStructure	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblDirectory	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblDirectoryPermission	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblDirectoryRelation	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblLookup	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblDirectorySubscriptions	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblSubscription	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblSubscriptionQuota	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblCartCarrier	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblCartOrderDelivery	 WHERE nAuditId = A.nAuditKey
UNION ALL 	SELECT COUNT(nAuditId) AS AuditCount FROM 	tblSubscriptionRenewal	 WHERE nAuditId = A.nAuditKey


) p)
 CNT
WHERE CNT.aCount > 2