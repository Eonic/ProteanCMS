---------------------------------------------------------------------------------------
	DECLARE @CartItemWIthInvalidAuditIds TABLE
	(
		nCartItemKey INT,
		nAuditId INT
	)

---------------------------------------------------------------------------------------

	--Duplicate AuditIds
	;WITH TEMP AS
	(
		SELECT
			*,
			ROW_NUMBER() OVER (PARTITION BY nAuditId ORDER BY nCartItemKey) Row_Num
		FROM tblCartItem C
		WHERE nAuditId >  0
	)

	--Get all content records with invalid Audit Ids
	INSERT INTO @CartItemWIthInvalidAuditIds
	SELECT C.nCartItemKey, C.nAuditId
	FROM 
	(
		--No AuditId
		SELECT nCartItemKey, nAuditId
		FROM tblCartItem C
		WHERE nAuditId IS NULL OR nAuditId = 0

		UNION

		--Invalid AuditId's. Does not exist in the tblAudit table.
		SELECT nCartItemKey, nAuditId
		FROM tblCartItem C
		WHERE NOT EXISTS
		(
			SELECT 1
			FROM tblAudit A
			WHERE A.nAuditKey = C.nAuditId
		)

		UNION

		SELECT nCartItemKey, nAuditId
		FROM TEMP
		WHERE Row_Num > 1
	) C
---------------------------------------------------------------------------------------

	SELECT *
	FROM @CartItemWIthInvalidAuditIds
	ORDER BY nAuditId


---------------------------------------------------------------------------------------
--Fix AuditIds

	DECLARE @nCartItemKey INT
	DECLARE @AuditId INT

    BEGIN TRY
        BEGIN TRAN

			DECLARE Cur CURSOR -- Create the cursor
			LOCAL FAST_FORWARD 
			-- set the type of cursor. Note you could also use READ_ONLY and FORWARD_ONLY. 
			-- You would have to performance test to see if you benifit from one or the other

			FOR
				SELECT
					nCartItemKey
				FROM @CartItemWIthInvalidAuditIds

			OPEN Cur
			FETCH NEXT FROM Cur INTO
			@nCartItemKey
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


				UPDATE tblCartItem
				SET nAuditId = @AuditId
				WHERE nCartItemKey = @nCartItemKey

				FETCH NEXT FROM Cur INTO @nCartItemKey

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


