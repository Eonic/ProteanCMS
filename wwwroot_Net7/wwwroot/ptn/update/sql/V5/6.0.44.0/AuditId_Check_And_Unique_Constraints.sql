--Description: Script to add Check and Unique key constraints on the AuditId column to avoid database inconsistencies

--Check Constraints
ALTER TABLE tblContent 
ADD CHECK (nAuditId > 0)

ALTER TABLE tblContentStructure 
ADD CHECK (nAuditId > 0)

ALTER TABLE tblContentLocation
ADD CHECK (nAuditId > 0)

ALTER TABLE tblCartItem 
ADD CHECK (nAuditId > 0)

ALTER TABLE tblDirectory
ADD CHECK (nAuditId > 0)


--Unique Keys
ALTER TABLE tblContent 
ADD UNIQUE(nAuditId)

ALTER TABLE tblContentStructure 
ADD UNIQUE(nAuditId)

ALTER TABLE tblContentLocation 
ADD UNIQUE(nAuditId)

ALTER TABLE tblCartItem 
ADD UNIQUE(nAuditId)

ALTER TABLE tblDirectory 
ADD UNIQUE(nAuditId)