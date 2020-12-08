---------------------------------------------------------------------------------------
	DECLARE @StructWIthInvalidAuditIds TABLE
	(
		nStructKey INT,
		nAuditId INT,
		isDuplicate BIT		
	)

---------------------------------------------------------------------------------------

	--Duplicate AuditIds
	;WITH TEMP AS
	(
		SELECT
			*,
			ROW_NUMBER() OVER (PARTITION BY nAuditId ORDER BY nStructKey) Row_Num
		FROM tblContentStructure C
		WHERE nAuditId >  0
	)

	--Get all content records with invalid Audit Ids
	INSERT INTO @StructWIthInvalidAuditIds
	SELECT C.nStructKey, C.nAuditId
	FROM 
	(
		--No AuditId
		SELECT nStructKey, nAuditId, 0 AS isDuplicate
		FROM tblContentStructure c 
		WHERE nAuditId IS NULL OR nAuditId = 0

		UNION

		--Invalid AuditId's. Does not exist in the tblAudit table.
		SELECT nStructKey, nAuditId, 0 AS isDuplicate
		FROM tblContentStructure C
		WHERE NOT EXISTS
		(
			SELECT 1
			FROM tblAudit A
			WHERE A.nAuditKey = C.nAuditId
		)

		UNION

		SELECT nStructKey, nAuditId, 1 AS isDuplicate
		FROM TEMP
		WHERE Row_Num > 1
	) C
---------------------------------------------------------------------------------------

	SELECT *
	FROM @StructWIthInvalidAuditIds
	ORDER BY nAuditId


---------------------------------------------------------------------------------------
--Fix AuditIds

	DECLARE @nStructKey INT
	DECLARE @originalAuditId INT
	DECLARE @newAuditId INT
	DECLARE @isDuplicate BIT
	DECLARE @auditStatus BIT

    BEGIN TRY
        BEGIN TRAN

			DECLARE Cur CURSOR -- Create the cursor
			LOCAL FAST_FORWARD 
			-- set the type of cursor. Note you could also use READ_ONLY and FORWARD_ONLY. 
			-- You would have to performance test to see if you benifit from one or the other

			FOR
				SELECT
					nStructKey,
					nAuditId,
					isDuplicate					
				FROM @StructWIthInvalidAuditIds

			OPEN Cur
			FETCH NEXT FROM Cur INTO
			@nStructKey, @originalAuditId, @isDuplicate
			-- Assigns values to variables declared at the top


			WHILE @@FETCH_STATUS = 0
			BEGIN
				SET @newAuditId = 0
				SET @auditStatus = 0 -- Default Audit status would be Hidden

				--For duplicate records - set the status of new audit record to what it was originally.
				IF @isDuplicate = 1
				BEGIN
					SELECT @auditStatus = nStatus
					FROM tblAudit
					WHERE nAuditKey = @originalAuditId
				END
	
				-- Inserts the Audit record 
				INSERT INTO tblAudit 
				SELECT 
					NULL AS dPublishDate, 
					NULL AS dExpireDate, 
					GETDATE() AS dInsertDate, 
					0 AS nInsertDirId, 
					GETDATE() AS dUpdateDate, 
					0 AS nUpdateDirId, 
					@auditStatus AS nStatus, 
					NULL AS cDescription

				SET @newAuditId = SCOPE_IDENTITY()


				UPDATE tblContentStructure
				SET nAuditId = newAuditId
				WHERE nStructKey = @nStructKey

				FETCH NEXT FROM Cur INTO @nStructKey

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


