--Description: Script to add Check and Unique key constraints on the AuditId column to avoid database inconsistencies

--Check Constraints
ALTER TABLE tblContent 
ADD CONSTRAINT CK_tblContent_nAuditId CHECK (nAuditId > 0)

ALTER TABLE tblContentStructure 
ADD CONSTRAINT CK_tblContentStructure_nAuditId CHECK (nAuditId > 0)

ALTER TABLE tblContentLocation
ADD CONSTRAINT CK_tblContentLocation_nAuditId CHECK (nAuditId > 0)

ALTER TABLE tblCartItem 
ADD CONSTRAINT CK_tblCartItem_nAuditId CHECK (nAuditId > 0)

ALTER TABLE tblDirectory
ADD CONSTRAINT CK_tblDirectory_nAuditId CHECK (nAuditId > 0)


--Unique Keys
ALTER TABLE tblContent 
ADD CONSTRAINT UQ_tblContent_nAuditId UNIQUE(nAuditId)

ALTER TABLE tblContentStructure 
ADD CONSTRAINT UQ_tblContentStructure_nAuditId UNIQUE(nAuditId)

ALTER TABLE tblContentLocation 
ADD CONSTRAINT UQ_tblContentLocation_nAuditId UNIQUE(nAuditId)

ALTER TABLE tblCartItem 
ADD CONSTRAINT UQ_tblCartItem_nAuditId UNIQUE(nAuditId)

ALTER TABLE tblDirectory 
ADD CONSTRAINT UQ_tblDirectory_nAuditId UNIQUE(nAuditId)