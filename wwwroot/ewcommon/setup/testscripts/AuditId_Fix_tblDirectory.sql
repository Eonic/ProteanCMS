---------------------------------------------------------------------------------------
	DECLARE @DirWIthInvalidAuditIds TABLE
	(
		nDirKey INT,
		nAuditId INT
	)

---------------------------------------------------------------------------------------

	--Duplicate AuditIds
	;WITH TEMP AS
	(
		SELECT
			*,
			ROW_NUMBER() OVER (PARTITION BY nAuditId ORDER BY nDirKey) Row_Num
		FROM tblDirectory D
		WHERE nAuditId >  0
	)

	--Get all content records with invalid Audit Ids
	INSERT INTO @DirWIthInvalidAuditIds
	SELECT D.nDirKey, D.nAuditId
	FROM 
	(
		--No AuditId
		SELECT nDirKey, nAuditId
		FROM tblDirectory D
		WHERE nAuditId IS NULL OR nAuditId = 0

		UNION

		--Invalid AuditId's. Does not exist in the tblAudit table.
		SELECT nDirKey, nAuditId
		FROM tblDirectory D
		WHERE NOT EXISTS
		(
			SELECT 1
			FROM tblAudit A
			WHERE A.nAuditKey = D.nAuditId
		)

		UNION

		SELECT nDirKey, nAuditId
		FROM TEMP
		WHERE Row_Num > 1
	) D
---------------------------------------------------------------------------------------

	SELECT *
	FROM @DirWIthInvalidAuditIds
	ORDER BY nAuditId


---------------------------------------------------------------------------------------
--Fix AuditIds

	DECLARE @nDirKey INT
	DECLARE @AuditId INT

    BEGIN TRY
        BEGIN TRAN

			DECLARE Cur CURSOR -- Create the cursor
			LOCAL FAST_FORWARD 
			-- set the type of cursor. Note you could also use READ_ONLY and FORWARD_ONLY. 
			-- You would have to performance test to see if you benifit from one or the other

			FOR
				SELECT
					nDirKey
				FROM @DirWIthInvalidAuditIds

			OPEN Cur
			FETCH NEXT FROM Cur INTO
			@nDirKey
			-- Assigns values to variables declared at the top


			WHILE @@FETCH_STATUS = 0
			BEGIN
				SET @AuditId = 0
	
				-- Inserts the Audit record 
				INSERT INTO tblAudit 
				SELECT 
					NULL AS dPublishDate, 
					NULL AS dExpireDate, 
					GETDATE() AS dInsertDate, 
					0 AS nInsertDirId, 
					GETDATE() AS dUpdateDate, 
					0 AS nUpdateDirId, 
					0 AS nStatus, 
					NULL AS cDescription

				SET @AuditId = SCOPE_IDENTITY()


				UPDATE tblDirectory
				SET nAuditId = @AuditId
				WHERE nDirKey = @nDirKey

				FETCH NEXT FROM Cur INTO @nDirKey

			END

			CLOSE Cur
			DEALLOCATE Cur


        COMMIT TRAN
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN
        THROW
    END CATCH



---------------------------------------------------------------------------------------


