IF COL_LENGTH('tblCartDiscountRules', 'nUseCount') IS NULL
BEGIN
    ALTER TABLE tblCartDiscountRules 
    ADD nUseCount INT NULL;
END
GO

-- Add nUseLimit column if not exists
IF COL_LENGTH('tblCartDiscountRules', 'nUseLimit') IS NULL
BEGIN
    ALTER TABLE tblCartDiscountRules 
    ADD nUseLimit INT NULL;
END
GO