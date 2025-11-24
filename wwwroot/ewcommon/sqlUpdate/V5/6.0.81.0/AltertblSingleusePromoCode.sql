-- Add DiscountId column if not exists
IF COL_LENGTH('tblSingleUsePromoCode', 'DiscountId') IS NULL
BEGIN
    ALTER TABLE tblSingleUsePromoCode 
    ADD DiscountId INT NULL;
END


-- Add nAuditId column if not exists
IF COL_LENGTH('tblSingleUsePromoCode', 'nAuditId') IS NULL
BEGIN
    ALTER TABLE tblSingleUsePromoCode 
    ADD nAuditId INT NULL;
END
