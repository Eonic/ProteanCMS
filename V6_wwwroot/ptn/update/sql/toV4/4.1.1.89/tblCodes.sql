/****** Object:  Table [dbo].[tblCodes]    Script Date: 03/16/2011 19:23:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
des
CREATE TABLE [dbo].[tblCodes](
	[nCodeKey] [int] IDENTITY(1,1) NOT NULL,
	[cCodeName] [nvarchar](50) NULL,
	[nCodeType] [int] NULL,
	[nCodeParentId] [int] NULL,
	[cCodeGroups] [nvarchar](50) NULL,
	[cCode] [nvarchar](255) NOT NULL,
	[nUseId] [int] NULL,
	[dUseDate] [datetime] NULL,
	[nAuditId] [int] NULL,
 CONSTRAINT [PK_tblCodes] PRIMARY KEY CLUSTERED 
(
	[nCodeKey] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

GO
/****** Object:  StoredProcedure [dbo].[spGetCodes]    Script Date: 03/16/2011 19:40:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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


/****** Object:  StoredProcedure [dbo].[spGetCodeDirectoryGroups]    Script Date: 03/16/2011 19:41:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[spGetCodeDirectoryGroups]
(
	@Codes nvarchar(255) = NULL
)
AS
BEGIN
	DECLARE @tblCodes TABLE (nCodeId int)
	INSERT INTO @tblCodes SELECT * FROM fxn_CSVTableINTEGERS(@Codes)
--SELECT * FROM @tblCodes


	DECLARE @tblGroups TABLE (nCodeKey int, nDirKey int, cDirName nvarchar(255)) 

	DECLARE curGroups CURSOR FOR SELECT nCodeId FROM @tblCodes
	DECLARE @CodeId int
		OPEN curGroups 
		FETCH NEXT FROM curGroups INTO @CodeId 
		WHILE (@@FETCH_STATUS = 0)
			BEGIN
				DECLARE @GroupsString nvarchar(255)
				SET @GroupsString = (SELECT cCodeGroups FROM tblCodes WHERE nCodeKey = @CodeId)
				INSERT INTO @tblGroups (nCodeKey, nDirKey, cDirName)
					SELECT @CodeId, groups.value, tblDirectory.cDirName
					FROM fxn_CSVTableINTEGERS(@GroupsString) groups
					LEFT OUTER JOIN tblDirectory ON groups.value = tblDirectory.nDirKey

--SELECT * FROM  fxn_CSVTableINTEGERS(@GroupsString) groups
--LEFT OUTER JOIN tblDirectory ON groups.value = tblDirectory.nDirKey

				FETCH NEXT FROM curGroups INTO @CodeId
			END
		CLOSE curGroups 
		DEALLOCATE curGroups
	--Return
	SELECT * FROM @tblGroups ORDER BY cDirName
END