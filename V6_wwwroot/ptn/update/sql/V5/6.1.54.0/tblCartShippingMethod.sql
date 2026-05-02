
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE Name = 'nShipOptWeightOverageRate'
      AND Object_ID = Object_ID('dbo.tblCartShippingMethods')
)
BEGIN
    ALTER TABLE dbo.tblCartShippingMethods ADD
	    nShipOptWeightOverageUnit money NULL,
	    nShipOptWeightOverageRate money NULL
END
ALTER TABLE dbo.tblCartShippingMethods SET (LOCK_ESCALATION = TABLE)

