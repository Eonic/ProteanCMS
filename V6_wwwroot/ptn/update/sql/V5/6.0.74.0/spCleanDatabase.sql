
CREATE PROCEDURE [dbo].[spCleanDatabase]

AS
Begin

Declare @RangeDate as Datetime

Select @RangeDate=DATEADD(yy,-1,getDate())
print @RangeDate

Begin Transaction

--truncating log table
truncate table tblActivityLog
truncate table tblEmailActivityLog

-- Deleting order data
delete from tblAudit where nAuditKey in (select nAuditId from tblCartItem where nCartOrderId in (select nCartOrderKey from tblCartOrder where nCartStatus=1 and dInsertDate<= @RangeDate));
delete from tblCartItem where nCartOrderId in (select  nCartOrderId from tblCartItem where nCartOrderId in (select nCartOrderKey from tblCartOrder where nCartStatus=1));
delete from tblAudit where nAuditKey in (select nAuditId from tblCartContact where nContactCartId in (select nCartOrderKey from tblCartOrder where nCartStatus=1) and dInsertDate<= @RangeDate);
delete from tblCartContact where nContactCartId in (select nCartOrderKey from tblCartOrder where nCartStatus=1);
delete from tblAudit where nAuditKey in (select nAuditId from tblCartOrder where nCartStatus=1 and dInsertDate<= @RangeDate);
delete from tblCartOrder where nCartStatus=1 

--deleting data from promo code
delete from tblAudit where nAuditKey in (select nAuditId from tblCartDiscountProdCatRelations where nDiscountId in (select nDiscountKey from tblCartDiscountRules where cDiscountCode like '%voucher') and dInsertDate<= @RangeDate)
delete from tblCartDiscountProdCatRelations where nDiscountId in (select nDiscountKey from tblCartDiscountRules where cDiscountCode like '%voucher')
delete from tblAudit where nAuditKey in (select nAuditId from tblCartDiscountDirRelations where nDiscountId in (select nDiscountKey from tblCartDiscountRules where cDiscountCode like '%voucher') and dInsertDate<= @RangeDate)
delete from tblCartDiscountDirRelations where nDiscountId in (select nDiscountKey from tblCartDiscountRules where cDiscountCode like '%voucher')
delete from tblAudit where nAuditkey in (select nAuditId from tblCartDiscountRules where cDiscountCode like '%voucher' and dInsertDate<= @RangeDate)
delete from tblCartDiscountRules where cDiscountCode like '%voucher'

Commit Transaction

end
