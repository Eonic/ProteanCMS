Select p.*, pm.*, a.dInsertDate from tblCartPayment p 
inner join tblCartPaymentMethod pm on p.nCartPaymentMethodId = pm.nPayMthdKey 
inner join tblAudit a on a.nAuditKey = p.nAuditId 
where nCartOrderId=516175