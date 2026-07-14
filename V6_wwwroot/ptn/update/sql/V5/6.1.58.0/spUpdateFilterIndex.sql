
CREATE PROCEDURE [dbo].[spUpdateFilterIndex] 
    @SchemaName VARCHAR(50),
    @IndexId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Variable declarations
    DECLARE @isParentRefForSKU INT;
    DECLARE @defName VARCHAR(200);
    DECLARE @fieldPath VARCHAR(MAX);
    DECLARE @cContentDataType INT;
    DECLARE @bBriefNotDetail BIT = 0;
    DECLARE @cDefaultValue VARCHAR(20);
    DECLARE @dataType VARCHAR(MAX);
    DECLARE @sql NVARCHAR(MAX);
    DECLARE @columnName NVARCHAR(50);
    DECLARE @xmlSource NVARCHAR(50);
    
    -- Get index definition
    SELECT 
        @isParentRefForSKU = bProductRefForSKU,
        @defName = cDefinitionName,
        @fieldPath = cContentValueXpath,
        @cContentDataType = nContentIndexDataType,
        @bBriefNotDetail = bBriefNotDetail,
        @cDefaultValue = ISNULL(cDefaultValue, '')
    FROM tblContentIndexDef
    WHERE nContentIndexDefKey = @IndexId;
    
    -- Validate required data was found
    IF @fieldPath IS NULL
    BEGIN
        RAISERROR('Index definition not found for IndexId %d', 16, 1, @IndexId);
        RETURN;
    END

    -- Set data type and target column based on content data type
    IF (@cContentDataType = 1) -- number        
    BEGIN
        SET @dataType = 'varchar(200)';  -- Always extract as varchar first
        SET @columnName = '[nNumberValue]';
    END
    ELSE IF (@cContentDataType = 2) -- text 
    BEGIN
        SET @dataType = 'varchar(200)';
        SET @columnName = '[cTextValue]';
    END
    ELSE IF (@cContentDataType = 3) -- date        
    BEGIN
        SET @dataType = 'varchar(200)';  -- Always extract as varchar first
        SET @columnName = '[dDateValue]';
    END
    ELSE
    BEGIN
        RAISERROR('Invalid content data type: %d', 16, 1, @cContentDataType);
        RETURN;
    END

    -- Determine XML source (Brief or Detail)
    SET @xmlSource = CASE WHEN @bBriefNotDetail = 1 THEN 'cContentXmlBrief' ELSE 'cContentXmlDetail' END;

    -- Handle SKU parent reference logic
    IF (@isParentRefForSKU = 1)
    BEGIN
        DECLARE @tblSkuAndValue TABLE (
            SKUId INT,
            nValue VARCHAR(200)
        );
        
        DECLARE @query NVARCHAR(MAX);

        -- Build query - always extract as varchar to avoid conversion errors
        SET @query = N'
            SELECT tc.nContentKey,
                   CONVERT(VARCHAR(200),
                       CONVERT(XML, tc.cContentXmlBrief).value(''' + @fieldPath + ''', ''varchar(200)'')
                   ) AS price
            FROM tblContent tc
            INNER JOIN tblAudit a ON tc.nAuditId = a.nAuditKey
            WHERE tc.cContentSchemaName = ''SKU'' 
              AND a.nStatus = 1';

        INSERT INTO @tblSkuAndValue
        EXEC sp_executesql @query;

        DECLARE @tblRelation TABLE (
            parentId INT,
            childId INT
        );
        
        DECLARE @strQuery NVARCHAR(MAX) = N'
            SELECT cr.nContentParentId, cr.nContentChildId
            FROM tblContentRelation cr
            INNER JOIN tblContent c ON cr.nContentParentId = c.nContentKey
            WHERE c.cContentSchemaName = ''Product'' 
              AND cr.nContentChildId IN (
                  SELECT tc.nContentKey
                  FROM tblContent tc
                  INNER JOIN tblAudit a ON tc.nAuditId = a.nAuditKey
                  WHERE tc.cContentSchemaName = ''SKU'' 
                    AND a.nStatus = 1
              )';

        INSERT INTO @tblRelation
        EXEC sp_executesql @strQuery;

        BEGIN TRANSACTION filterIndex;

        BEGIN TRY
            DELETE FROM tblContentIndex
            WHERE nContentIndexDefinitionKey = @IndexId;

            -- Insert based on data type
            IF (@cContentDataType = 1) -- number
            BEGIN
                -- Pre-convert default value
                DECLARE @defaultFloatSKU FLOAT = ISNULL(TRY_CONVERT(FLOAT, NULLIF(@cDefaultValue, '')), 0);

                INSERT INTO tblContentIndex (
                    nContentId,
                    nContentIndexDefinitionKey,
                    nNumberValue
                )
                SELECT 
                    tr.parentId,
                    @IndexId,
                    ISNULL(
                        TRY_CONVERT(FLOAT, sp.nValue),
                        @defaultFloatSKU
                    )
                FROM @tblRelation tr
                INNER JOIN @tblSkuAndValue sp ON tr.childId = sp.SKUId
                WHERE sp.nValue IS NOT NULL 
                  AND sp.nValue <> ''
                  AND (TRY_CONVERT(FLOAT, sp.nValue) IS NOT NULL OR @cDefaultValue <> '');
            END
            ELSE IF (@cContentDataType = 2) -- text         
            BEGIN
                INSERT INTO tblContentIndex (
                    nContentId,
                    nContentIndexDefinitionKey,
                    cTextValue
                )
                SELECT 
                    tr.parentId,
                    @IndexId,
                    ISNULL(NULLIF(sp.nValue, ''), @cDefaultValue)
                FROM @tblRelation tr
                INNER JOIN @tblSkuAndValue sp ON tr.childId = sp.SKUId
                WHERE sp.nValue IS NOT NULL 
                  AND (sp.nValue <> '' OR @cDefaultValue <> '');
            END
            ELSE IF (@cContentDataType = 3) -- date         
            BEGIN
                -- Pre-convert default value
                DECLARE @defaultDateSKU DATETIME = TRY_CONVERT(DATETIME, NULLIF(@cDefaultValue, ''));

                INSERT INTO tblContentIndex (
                    nContentId,
                    nContentIndexDefinitionKey,
                    dDateValue
                )
                SELECT 
                    tr.parentId,
                    @IndexId,
                    ISNULL(
                        TRY_CONVERT(DATETIME, sp.nValue),
                        @defaultDateSKU
                    )
                FROM @tblRelation tr
                INNER JOIN @tblSkuAndValue sp ON tr.childId = sp.SKUId
                WHERE sp.nValue IS NOT NULL 
                  AND sp.nValue <> ''
                  AND (TRY_CONVERT(DATETIME, sp.nValue) IS NOT NULL OR @defaultDateSKU IS NOT NULL);
            END

            COMMIT TRANSACTION filterIndex;
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION filterIndex;
            
            DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
            DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
            DECLARE @ErrorState INT = ERROR_STATE();
            
            RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
        END CATCH
    END
    ELSE
    BEGIN
        -- Handle standard content indexing
        BEGIN TRANSACTION contentIndex;

        BEGIN TRY
            DELETE FROM tblContentIndex
            WHERE nContentIndexDefinitionKey = @IndexId;

            -- Build and execute dynamic SQL based on data type
            IF (@cContentDataType = 1) -- number/float
            BEGIN
                -- Pre-convert default value
                DECLARE @defaultFloat FLOAT = ISNULL(TRY_CONVERT(FLOAT, NULLIF(@cDefaultValue, '')), 0);

                SET @sql = N'
                    INSERT INTO tblContentIndex (
                        nContentId,
                        nContentIndexDefinitionKey,
                        nNumberValue
                    )
                    SELECT 
                        c.nContentKey,
                        @IndexId,
                        ISNULL(
                            TRY_CONVERT(FLOAT, 
                                CONVERT(XML, c.' + @xmlSource + N').value(''' + @fieldPath + N''', ''varchar(200)'')
                            ),
                            @defaultFloat
                        )
                    FROM tblContent c
                    WHERE c.cContentSchemaName = @SchemaName
                      AND CONVERT(XML, c.' + @xmlSource + N').exist(''' + @fieldPath + N''') = 1
                      AND NULLIF(CONVERT(XML, c.' + @xmlSource + N').value(''' + @fieldPath + N''', ''varchar(200)''), '''') IS NOT NULL';

                EXEC sp_executesql 
                    @sql,
                    N'@IndexId INT, @defaultFloat FLOAT, @SchemaName VARCHAR(50)',
                    @IndexId = @IndexId,
                    @defaultFloat = @defaultFloat,
                    @SchemaName = @SchemaName;
            END
            ELSE IF (@cContentDataType = 2) -- text
            BEGIN
                SET @sql = N'
                    INSERT INTO tblContentIndex (
                        nContentId,
                        nContentIndexDefinitionKey,
                        cTextValue
                    )
                    SELECT 
                        c.nContentKey,
                        @IndexId,
                        ISNULL(
                            NULLIF(
                                CONVERT(XML, c.' + @xmlSource + N').value(''' + @fieldPath + N''', ''varchar(20)''),
                                ''''
                            ),
                            @cDefaultValue
                        )
                    FROM tblContent c
                    WHERE c.cContentSchemaName = @SchemaName
                      AND CONVERT(XML, c.' + @xmlSource + N').exist(''' + @fieldPath + N''') = 1
                      AND (NULLIF(CONVERT(XML, c.' + @xmlSource + N').value(''' + @fieldPath + N''', ''varchar(20)''), '''') IS NOT NULL
                           OR @cDefaultValue <> '''')';

                EXEC sp_executesql 
                    @sql,
                    N'@IndexId INT, @cDefaultValue VARCHAR(20), @SchemaName VARCHAR(50)',
                    @IndexId = @IndexId,
                    @cDefaultValue = @cDefaultValue,
                    @SchemaName = @SchemaName;
            END
            ELSE IF (@cContentDataType = 3) -- date
            BEGIN
                -- Pre-convert default value
                DECLARE @defaultDate DATETIME = TRY_CONVERT(DATETIME, NULLIF(@cDefaultValue, ''));

                SET @sql = N'
                    INSERT INTO tblContentIndex (
                        nContentId,
                        nContentIndexDefinitionKey,
                        dDateValue
                    )
                    SELECT 
                        c.nContentKey,
                        @IndexId,
                        ISNULL(
                            TRY_CONVERT(DATETIME, 
                                CONVERT(XML, c.' + @xmlSource + N').value(''' + @fieldPath + N''', ''varchar(200)'')
                            ),
                            @defaultDate
                        )
                    FROM tblContent c
                    WHERE c.cContentSchemaName = @SchemaName
                      AND CONVERT(XML, c.' + @xmlSource + N').exist(''' + @fieldPath + N''') = 1
                      AND NULLIF(CONVERT(XML, c.' + @xmlSource + N').value(''' + @fieldPath + N''', ''varchar(200)''), '''') IS NOT NULL';

                EXEC sp_executesql 
                    @sql,
                    N'@IndexId INT, @defaultDate DATETIME, @SchemaName VARCHAR(50)',
                    @IndexId = @IndexId,
                    @defaultDate = @defaultDate,
                    @SchemaName = @SchemaName;
            END

            COMMIT TRANSACTION contentIndex;
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION contentIndex;
            
            DECLARE @ErrorMsg NVARCHAR(4000) = ERROR_MESSAGE();
            DECLARE @ErrorSev INT = ERROR_SEVERITY();
            DECLARE @ErrorSt INT = ERROR_STATE();
            
            RAISERROR(@ErrorMsg, @ErrorSev, @ErrorSt);
        END CATCH
    END
END



