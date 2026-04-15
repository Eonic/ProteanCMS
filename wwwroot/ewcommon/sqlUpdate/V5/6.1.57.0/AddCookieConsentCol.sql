IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE Name = 'bCookieConsentEnabled'
      AND Object_ID = Object_ID('tblCartOrder')
)
BEGIN
    ALTER TABLE tblCartOrder
    ADD bCookieConsentEnabled BIT;
END