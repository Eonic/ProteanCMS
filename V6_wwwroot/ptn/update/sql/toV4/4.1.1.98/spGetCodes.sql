
	CREATE  PROCEDURE [dbo].[spGetCodes]
	(
		@type int = NULL
	)
	AS
	BEGIN
		SELECT nCodeKey,cCodeName,nCodeType,cCodeGroups ,nCodeParentId,tblAudit.dPublishDate,tblAudit.dExpireDate,tblAudit.nStatus,
		((SELECT Count(child.nCodeKey) FROM tblCodes child WHERE (child.nCodeParentId = Codes.nCodeKey) AND ((child.nUseId IS NULL) OR (child.nUseId = 0) ) )) AS nUnused,
		((SELECT Count(child.nCodeKey) FROM tblCodes child WHERE child.nCodeParentId = Codes.nCodeKey AND child.nUseId > 0 )) AS nUsed
		FROM tblCodes Codes INNER JOIN tblAudit ON Codes.nAuditId = tblAudit.nAuditKey
		WHERE 
			Codes.nCodeType = 
				CASE WHEN @type IS NULL
					THEN 
						Codes.nCodeType
					ELSE
						@type
				END
	AND (nCodeParentId IS NULL or nCodeParentId = 0)
		ORDER BY Codes.nCodeType, Codes.cCodeName
	END
