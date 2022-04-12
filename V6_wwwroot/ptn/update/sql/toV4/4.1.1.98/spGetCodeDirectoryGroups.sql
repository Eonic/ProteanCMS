
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
