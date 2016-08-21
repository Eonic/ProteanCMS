CREATE VIEW dbo.vw_CurrentLiveContentIdsAndSchemas AS
SELECT 
		content.nContentKey,
		content.cContentSchemaName
FROM   
		tblContent content
		INNER JOIN tblAudit audit
			ON	content.nAuditId = audit.nAuditKey
				AND audit.nStatus = 1
				AND (audit.dExpireDate IS NULL OR audit.dExpireDate=0 OR audit.dExpireDate >= CAST(FLOOR(CAST(GETDATE() AS FLOAT)) AS DATETIME))
				AND (audit.dPublishDate IS NULL OR audit.dPublishDate=0 OR audit.dPublishDate <= CAST(FLOOR(CAST(GETDATE() AS FLOAT)) AS DATETIME))
			



