CREATE PROCEDURE [dbo].[spGetProductShippingOptions]
-- Parameters for the stored procedure
@ProductId BIGINT,
@Currency NVARCHAR(3) = '',
@userId BIGINT = 0,
@AuthUsers BIGINT = 0,
@NonAuthUsers BIGINT = 0,
@CountryList NVARCHAR(1000) = '',
@dValidDate Date = NULL,
@GroupType NVARCHAR(100) = 'Shipping',
@ProductPrice FLOAT = 0

AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

    -- Set default date if not provided
    IF @dValidDate IS NULL
        SET @dValidDate = GETDATE();

    DECLARE @strMainQuery NVARCHAR(MAX) = ''
    DECLARE @strFirstQuery NVARCHAR(MAX) = ''
    DECLARE @strSecondQuery NVARCHAR(MAX) = ''
    DECLARE @strEndConditionQuery NVARCHAR(MAX) = ''
    DECLARE @strCountryConditionQuery NVARCHAR(MAX) = ''
    DECLARE @shippingGroupCondition NVARCHAR(MAX) = ''
    DECLARE @shippingGroupRuleTypeCondition NVARCHAR(MAX) = ''
    DECLARE @ShippingGroupName NVARCHAR(MAX) = ''
    DECLARE @ShippingGroupCatIDList NVARCHAR(MAX) = ''
    DECLARE @ExistShippingGroupCount INT = 0

    -- Base query to get shipping options
    SET @strFirstQuery = 'SELECT DISTINCT opt.nShipOptKey, opt.cCurrency, opt.cShipOptName, opt.cShipOptForeignRef, opt.cShipOptCarrier, opt.cShipOptTime, 
        CAST(opt.cShipOptTandC AS NVARCHAR(MAX)) AS cShipOptTandC,
        opt.nShipOptCost, opt.nShipOptPercentage, opt.nShipOptQuantMin, opt.nShipOptQuantMax, opt.nShipOptWeightMin, opt.nShipOptWeightMax,
        opt.nShipOptPriceMin, opt.nShipOptPriceMax, opt.nShipOptHandlingPercentage, opt.nShipOptHandlingFixedCost, opt.nShipOptTaxRate,
        opt.nAuditId, opt.nDisplayPriority, opt.bCollection, opt.nShipOptCat, opt.nShipOptCost as nShippingTotal,
        '''' AS nShippingGroup, opt.bOverrideForWholeOrder,
        opt.nShipOptWeightOverageUnit, opt.nShipOptWeightOverageRate,
        Loc.cLocationNameShort
        FROM tblCartShippingMethods opt
        INNER JOIN tblAudit ON opt.nAuditId = tblAudit.nAuditKey
        LEFT JOIN tblCartShippingRelations rel ON opt.nShipOptKey = rel.nShpOptId
        LEFT JOIN tblCartShippingLocations Loc ON rel.nShpLocId = Loc.nLocationKey '

    -- Permission and currency conditions
    SET @strSecondQuery = 'WHERE ((opt.cCurrency Is Null) OR (opt.cCurrency = '''') OR (opt.cCurrency = ''' + @Currency + '''))
        AND (opt.bCollection = 1 OR opt.nShipOptKey =
        CASE WHEN ' + CONVERT(NVARCHAR(10), @userId) + ' = 0 THEN 
            (CASE WHEN (SELECT COUNT(perm.nCartShippingPermissionKey) FROM tblCartShippingPermission perm
                WHERE perm.nShippingMethodId = opt.nShipOptKey
                AND perm.nDirId = ' + CONVERT(NVARCHAR(10), @NonAuthUsers) + ' AND perm.nPermLevel = 1) > 0
                OR (SELECT COUNT(*) FROM tblCartShippingPermission perm WHERE opt.nShipOptKey = perm.nShippingMethodId AND perm.nPermLevel = 1) = 0
                THEN opt.nShipOptKey END)
        WHEN ' + CONVERT(NVARCHAR(10), @userId) + ' > 0 THEN
            (CASE WHEN ((SELECT COUNT(perm.nCartShippingPermissionKey) FROM tblCartShippingPermission perm
                INNER JOIN tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId
                WHERE perm.nShippingMethodId = opt.nShipOptKey AND PermGroup.nDirChildId = ' + CONVERT(NVARCHAR(10), @userId) + ' AND perm.nPermLevel = 1) > 0
                AND NOT ((SELECT COUNT(perm.nCartShippingPermissionKey) FROM tblCartShippingPermission perm
                INNER JOIN tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId
                WHERE perm.nShippingMethodId = opt.nShipOptKey AND PermGroup.nDirChildId = ' + CONVERT(NVARCHAR(10), @userId) + ' AND perm.nPermLevel = 0) > 0)
                OR (SELECT COUNT(perm.nCartShippingPermissionKey) FROM tblCartShippingPermission perm
                WHERE perm.nShippingMethodId = opt.nShipOptKey AND perm.nDirId = ' + CONVERT(NVARCHAR(10), @AuthUsers) + ' AND perm.nPermLevel = 1) > 0
                OR (SELECT COUNT(*) FROM tblCartShippingPermission perm WHERE opt.nShipOptKey = perm.nShippingMethodId AND perm.nPermLevel = 1) = 0)
                AND opt.nShipOptKey NOT IN (SELECT nShippingMethodId FROM tblCartShippingPermission perm
                INNER JOIN tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId
                AND nPermLevel = 0 AND PermGroup.nDirChildId = ' + CONVERT(NVARCHAR(10), @userId) + ')
                THEN opt.nShipOptKey END)
        END)
        AND (opt.bCollection = 1 OR Loc.nLocationKey IS NOT NULL) '

    -- Country condition
    IF @CountryList <> ''
    BEGIN
        SET @strCountryConditionQuery = 'AND (opt.bCollection = 1 OR (loc.cLocationNameShort IN ' + @CountryList + ') OR (loc.cLocationNameFull IN ' + @CountryList + ')) '
    END

    -- Price condition - filter by product price if provided
    DECLARE @strPriceConditionQuery NVARCHAR(MAX) = ''
    IF @ProductPrice > 0
    BEGIN
        SET @strPriceConditionQuery = 'AND (
            (opt.nShipOptPriceMin IS NULL OR opt.nShipOptPriceMin = 0 OR opt.nShipOptPriceMin <= ' + CONVERT(NVARCHAR(50), @ProductPrice) + ')
            AND (opt.nShipOptPriceMax IS NULL OR opt.nShipOptPriceMax = 0 OR opt.nShipOptPriceMax >= ' + CONVERT(NVARCHAR(50), @ProductPrice) + ')
        ) '
    END

    -- Date and status conditions
    SET @strEndConditionQuery = 'AND (tblAudit.nStatus > 0) 
        AND ((tblAudit.dPublishDate = 0) OR (tblAudit.dPublishDate Is Null) OR (tblAudit.dPublishDate <= ''' + CONVERT(NVARCHAR(50), @dValidDate) + '''))
        AND ((tblAudit.dExpireDate = 0) OR (tblAudit.dExpireDate Is Null) OR (tblAudit.dExpireDate >= ''' + CONVERT(NVARCHAR(50), @dValidDate) + ''')) '

    -- Temporary table for shipping groups
    CREATE TABLE #ShippingGroupList (nCatKey BIGINT, cCatName NVARCHAR(250), cCatSchemaName NVARCHAR(250))
    DECLARE @ShippingGroupCatIDs TABLE (nCatId INT)

    -- Get product's shipping group categories
    IF @ProductId > 0
    BEGIN
        INSERT INTO #ShippingGroupList
        SELECT DISTINCT cpc.nCatKey, cpc.cCatName, cpc.cCatSchemaName
        FROM tblContent p
        INNER JOIN tblCartCatProductRelations cpr ON p.nContentKey = cpr.nContentId
        INNER JOIN tblCartProductCategories cpc ON cpr.nCatId = cpc.nCatKey
        WHERE p.nContentKey = @ProductId

        UNION

        SELECT DISTINCT cpc.nCatKey, cpc.cCatName, cpc.cCatSchemaName
        FROM tblContentRelation cr
        INNER JOIN tblCartCatProductRelations cpr ON cr.nContentParentId = cpr.nContentId
        INNER JOIN tblCartProductCategories cpc ON cpr.nCatId = cpc.nCatKey
        INNER JOIN tblContent p ON cr.nContentChildId = p.nContentKey
        WHERE cr.nContentChildId = @ProductId AND p.cContentSchemaName = 'SKU'

        SET @ExistShippingGroupCount = (SELECT COUNT(*) FROM #ShippingGroupList WHERE cCatSchemaName = @GroupType)

        -- Get shipping group category IDs
        IF @ExistShippingGroupCount > 0
        BEGIN
            INSERT INTO @ShippingGroupCatIDs (nCatId)
            SELECT DISTINCT cspc.nCatId 
            FROM tblCartShippingMethods opt
            INNER JOIN tblCartShippingProductCategoryRelations CSPC ON opt.nShipOptKey = CSPC.nShipOptId
            INNER JOIN tblCartCatProductRelations cpr ON CSPC.nCatId = cpr.nCatId 
            WHERE CSPC.nRuleType = 1 
            AND cspc.nCatId IN (SELECT DISTINCT nCatKey FROM #ShippingGroupList WHERE cCatSchemaName = @GroupType)

            SET @ShippingGroupName = (SELECT TOP 1 cCatName FROM #ShippingGroupList WHERE cCatSchemaName = @GroupType AND nCatKey = (SELECT TOP 1 nCatId FROM @ShippingGroupCatIDs ORDER BY 1 DESC))

            -- Build shipping group category ID list
            SELECT @ShippingGroupCatIDList = STUFF((
                SELECT DISTINCT ',' + CAST(nCatId AS NVARCHAR)
                FROM @ShippingGroupCatIDs
                FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)'), 1, 1, '')

            -- Guard: @ShippingGroupCatIDList is NULL when no shipping methods are mapped to the
            -- product's group categories. Using + concatenation with NULL would silently nullify
            -- @strEndConditionQuery and remove all filters, returning every method. Instead,
            -- force no results so a product in a group with no mapped methods returns nothing.
            IF @ShippingGroupCatIDList IS NOT NULL AND @ShippingGroupCatIDList <> ''
            BEGIN
                -- Collection methods (bCollection=1) are always shown regardless of group
                -- mapping so they appear on every product page.
                SET @strEndConditionQuery = @strEndConditionQuery + 'AND (opt.bCollection = 1 OR opt.nShipOptKey IN (
                    SELECT DISTINCT CSPC.nShipOptId
                    FROM tblCartShippingProductCategoryRelations CSPC
                    INNER JOIN tblCartCatProductRelations cpr ON CSPC.nCatId = cpr.nCatId
                    INNER JOIN tblCartShippingMethods csm ON CSPC.nShipOptId = csm.nShipOptKey
                    WHERE CSPC.nRuleType = 1 AND CSPC.nCatId IN (' + @ShippingGroupCatIDList + '))) '
            END
            ELSE
            BEGIN
                -- No methods mapped to this group: suppress results but keep collection options
                SET @strEndConditionQuery = @strEndConditionQuery + 'AND (opt.bCollection = 1 OR 1 = 0) '
            END
        END
        ELSE
        BEGIN
            -- Product has no shipping group: exclude methods exclusively mapped to any group
            -- (nRuleType=1). Collection methods (bCollection=1) are exempt and always shown.
            SET @strEndConditionQuery = @strEndConditionQuery + 'AND (opt.bCollection = 1 OR opt.nShipOptKey NOT IN (
                SELECT DISTINCT CSPC.nShipOptId
                FROM tblCartShippingProductCategoryRelations CSPC
                WHERE CSPC.nRuleType = 1)) '
        END  -- END ELSE of IF @ExistShippingGroupCount > 0
    END  -- END IF @ProductId > 0

    -- Build final query
    SET @strMainQuery = CONCAT(@strFirstQuery, @strSecondQuery, @strCountryConditionQuery, @strPriceConditionQuery, @strEndConditionQuery)

    -- Single-pass deduplication: MAX() OVER() computes the override flag without re-evaluating
    -- the CTE a second time (avoiding UNION double-evaluation and float rounding duplicates).
    -- ROW_NUMBER() OVER (PARTITION BY nShipOptKey) collapses multiple location rows per method.
    SET @strMainQuery = 'WITH ShippingOptions AS (
        ' + @strMainQuery + '
    ),
    Ranked AS (
        SELECT *,
               MAX(CASE WHEN ISNULL(bCollection, 0) = 0 THEN CAST(ISNULL(bOverrideForWholeOrder, 0) AS INT) ELSE 0 END) OVER () AS _hasOverride,
               ROW_NUMBER() OVER (PARTITION BY nShipOptKey ORDER BY nDisplayPriority, nShippingTotal, cLocationNameShort) AS _rn
        FROM ShippingOptions
    )
    SELECT nShipOptKey, cCurrency, cShipOptName, cShipOptForeignRef, cShipOptCarrier, cShipOptTime, cShipOptTandC,
           nShipOptCost, nShipOptPercentage, nShipOptQuantMin, nShipOptQuantMax, nShipOptWeightMin, nShipOptWeightMax,
           nShipOptPriceMin, nShipOptPriceMax, nShipOptHandlingPercentage, nShipOptHandlingFixedCost, nShipOptTaxRate,
           nAuditId, nDisplayPriority, bCollection, nShipOptCat, nShippingTotal, nShippingGroup,
           bOverrideForWholeOrder, nShipOptWeightOverageUnit, nShipOptWeightOverageRate, cLocationNameShort
    FROM Ranked
    WHERE _rn = 1 AND (bCollection = 1 OR _hasOverride = 0 OR bOverrideForWholeOrder = 1)
    ORDER BY nDisplayPriority, nShippingTotal'

    -- Execute the query
    DECLARE @ValidShippingOptions AS TABLE (
        nShipOptKey INT, cCurrency NVARCHAR(10), cShipOptName NVARCHAR(255), cShipOptForeignRef NVARCHAR(255), 
        cShipOptCarrier NVARCHAR(100), cShipOptTime NVARCHAR(50), cShipOptTandC NVARCHAR(MAX), 
        nShipOptCost MONEY, nShipOptPercentage FLOAT, nShipOptQuantMin FLOAT, nShipOptQuantMax FLOAT,
        nShipOptWeightMin FLOAT, nShipOptWeightMax FLOAT, nShipOptPriceMin FLOAT, nShipOptPriceMax FLOAT,
        nShipOptHandlingPercentage FLOAT, nShipOptHandlingFixedCost FLOAT, nShipOptTaxRate FLOAT,
        nAuditId INT, nDisplayPriority INT, bCollection BIT, nShipOptCat INT, nShippingTotal FLOAT,
        nShippingGroup NVARCHAR(500), bOverrideForWholeOrder INT, nShipOptWeightOverageUnit FLOAT,
        nShipOptWeightOverageRate FLOAT, cLocationNameShort NVARCHAR(100)
    )

    INSERT INTO @ValidShippingOptions
    EXEC (@strMainQuery)

    -- Update shipping group name if applicable
    IF @ShippingGroupName <> ''
    BEGIN
        UPDATE @ValidShippingOptions SET nShippingGroup = @ShippingGroupName
    END

    SELECT * FROM @ValidShippingOptions

    -- Cleanup
    DROP TABLE #ShippingGroupList
END
