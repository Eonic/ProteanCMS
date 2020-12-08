---------------------------------------------------------------------------------------
	DECLARE @ContentWIthInvalidAuditIds TABLE
	(
		nContentKey INT,
		nAuditId INT
	)

---------------------------------------------------------------------------------------

	--Duplicate AuditIds
	;WITH TEMP AS
	(
		SELECT
			*,
			ROW_NUMBER() OVER (PARTITION BY nAuditId ORDER BY nContentKey) Row_Num
		FROM tblContent C
		WHERE nAuditId >  0
	)

	--Get all content records with invalid Audit Ids
	INSERT INTO @ContentWIthInvalidAuditIds
	SELECT C.nContentKey, C.nAuditId
	FROM 
	(
		--No AuditId
		SELECT nContentKey, nAuditId
		FROM tblContent c 
		WHERE nAuditId IS NULL OR nAuditId = 0

		UNION

		--Invalid AuditId's. Does not exist in the tblAudit table.
		SELECT nContentKey, nAuditId
		FROM tblContent C
		WHERE NOT EXISTS
		(
			SELECT 1
			FROM tblAudit A
			WHERE A.nAuditKey = C.nAuditId
		)

		UNION

		SELECT nContentKey, nAuditId
		FROM TEMP
		WHERE Row_Num > 1
	) C
---------------------------------------------------------------------------------------

	SELECT *
	FROM @ContentWIthInvalidAuditIds
	ORDER BY nAuditId


---------------------------------------------------------------------------------------
--Fix AuditIds

	DECLARE @nContentKey INT
	DECLARE @AuditId INT

    BEGIN TRY
        BEGIN TRAN

			DECLARE Cur CURSOR -- Create the cursor
			LOCAL FAST_FORWARD 
			-- set the type of cursor. Note you could also use READ_ONLY and FORWARD_ONLY. 
			-- You would have to performance test to see if you benifit from one or the other

			FOR
				SELECT
					nContentKey
				FROM @ContentWIthInvalidAuditIds

			OPEN Cur
			FETCH NEXT FROM Cur INTO
			@nContentKey
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


				UPDATE tblContent
				SET nAuditId = @AuditId
				WHERE nContentKey = @nContentKey

				FETCH NEXT FROM Cur INTO @nContentKey

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


