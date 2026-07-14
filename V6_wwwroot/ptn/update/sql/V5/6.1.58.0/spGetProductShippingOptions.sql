CREATE OR ALTER PROCEDURE dbo.spGetProductShippingOptions
(
    @ProductId BIGINT,
    @Currency NVARCHAR(3) = '',
    @UserId BIGINT = 0,
    @AuthUsers BIGINT = 0,
    @NonAuthUsers BIGINT = 0,
    @CountryList NVARCHAR(1000) = '',
    @dValidDate DATE = NULL,
    @GroupType NVARCHAR(100) = 'Shipping',
    @ProductPrice FLOAT = 0,
    @Debug BIT = 0
)
AS
BEGIN

    SET NOCOUNT ON;

    IF @dValidDate IS NULL
        SET @dValidDate = GETDATE();

    ------------------------------------------------------------
    -- COUNTRIES
    ------------------------------------------------------------

   
CREATE TABLE #CountryFilter
(
    CountryCode NVARCHAR(100) PRIMARY KEY,
    SortOrder INT
);

INSERT INTO #CountryFilter
(
    CountryCode,
    SortOrder
)
SELECT
    LTRIM(RTRIM(REPLACE(value,'''',''))),
    ROW_NUMBER() OVER (ORDER BY (SELECT NULL))
FROM STRING_SPLIT(@CountryList, ',');


    ------------------------------------------------------------
    -- SHIPPING GROUPS
    ------------------------------------------------------------

    
------------------------------------------------------------
-- SHIPPING GROUPS
------------------------------------------------------------

CREATE TABLE #ShippingGroups
(
    nShipOptId BIGINT PRIMARY KEY
);

CREATE TABLE #GroupRestrictedMethods
(
    nShipOptId BIGINT PRIMARY KEY
);

CREATE TABLE #ShippingGroupList
(
    nCatKey BIGINT,
    cCatName NVARCHAR(250),
    cCatSchemaName NVARCHAR(250)
);

DECLARE @HasShippingGroup BIT = 0;

IF @ProductId > 0
BEGIN

    INSERT INTO #ShippingGroupList
    SELECT DISTINCT
        cpc.nCatKey,
        cpc.cCatName,
        cpc.cCatSchemaName
    FROM tblContent p
    INNER JOIN tblCartCatProductRelations cpr
        ON p.nContentKey = cpr.nContentId
    INNER JOIN tblCartProductCategories cpc
        ON cpr.nCatId = cpc.nCatKey
    WHERE p.nContentKey = @ProductId

    UNION

    SELECT DISTINCT
        cpc.nCatKey,
        cpc.cCatName,
        cpc.cCatSchemaName
    FROM tblContentRelation cr
    INNER JOIN tblCartCatProductRelations cpr
        ON cr.nContentParentId = cpr.nContentId
    INNER JOIN tblCartProductCategories cpc
        ON cpr.nCatId = cpc.nCatKey
    INNER JOIN tblContent p
        ON cr.nContentChildId = p.nContentKey
    WHERE cr.nContentChildId = @ProductId
      AND p.cContentSchemaName = 'SKU';

    IF EXISTS
    (
        SELECT 1
        FROM #ShippingGroupList
        WHERE cCatSchemaName = @GroupType
    )
    BEGIN

        SET @HasShippingGroup = 1;

        CREATE TABLE #ShippingGroupCatIDs
        (
            nCatId BIGINT PRIMARY KEY
        );

        INSERT INTO #ShippingGroupCatIDs
        (
            nCatId
        )
        SELECT DISTINCT
            CSPC.nCatId
        FROM tblCartShippingMethods opt
        INNER JOIN tblCartShippingProductCategoryRelations CSPC
            ON opt.nShipOptKey = CSPC.nShipOptId
        INNER JOIN tblCartCatProductRelations cpr
            ON CSPC.nCatId = cpr.nCatId
        WHERE CSPC.nRuleType = 1
        AND CSPC.nCatId IN
        (
            SELECT nCatKey
            FROM #ShippingGroupList
            WHERE cCatSchemaName = @GroupType
        );

        INSERT INTO #ShippingGroups
        (
            nShipOptId
        )
        SELECT DISTINCT
            CSPC.nShipOptId
        FROM tblCartShippingProductCategoryRelations CSPC
        INNER JOIN #ShippingGroupCatIDs sg
            ON sg.nCatId = CSPC.nCatId
        WHERE CSPC.nRuleType = 1;
    END;
END;

INSERT INTO #GroupRestrictedMethods
(
    nShipOptId
)
SELECT DISTINCT
    nShipOptId
FROM tblCartShippingProductCategoryRelations
WHERE nRuleType = 1;


    ------------------------------------------------------------
    -- ALLOWED METHODS
    ------------------------------------------------------------

    CREATE TABLE #AllowedMethods
    (
        nShipOptId BIGINT PRIMARY KEY
    );

    IF @UserId = 0
    BEGIN

        INSERT #AllowedMethods
        (
            nShipOptId
        )
        SELECT
            sm.nShipOptKey
        FROM tblCartShippingMethods sm
        WHERE

            EXISTS
            (
                SELECT 1
                FROM tblCartShippingPermission perm
                WHERE perm.nShippingMethodId = sm.nShipOptKey
                AND perm.nDirId = @NonAuthUsers
                AND perm.nPermLevel = 1
            )

            OR

            NOT EXISTS
            (
                SELECT 1
                FROM tblCartShippingPermission perm
                WHERE perm.nShippingMethodId = sm.nShipOptKey
                AND perm.nPermLevel = 1
            );

    END
    ELSE
    BEGIN

        INSERT #AllowedMethods
        (
            nShipOptId
        )
        SELECT
            sm.nShipOptKey
        FROM tblCartShippingMethods sm
        WHERE

            (
                EXISTS
                (
                    SELECT 1
                    FROM tblCartShippingPermission perm
                    INNER JOIN tblDirectoryRelation dr
                        ON dr.nDirParentId = perm.nDirId
                    WHERE perm.nShippingMethodId = sm.nShipOptKey
                    AND dr.nDirChildId = @UserId
                    AND perm.nPermLevel = 1
                )

                AND

                NOT EXISTS
                (
                    SELECT 1
                    FROM tblCartShippingPermission perm
                    INNER JOIN tblDirectoryRelation dr
                        ON dr.nDirParentId = perm.nDirId
                    WHERE perm.nShippingMethodId = sm.nShipOptKey
                    AND dr.nDirChildId = @UserId
                    AND perm.nPermLevel = 0
                )
            )

            OR

            EXISTS
            (
                SELECT 1
                FROM tblCartShippingPermission perm
                WHERE perm.nShippingMethodId = sm.nShipOptKey
                AND perm.nDirId = @AuthUsers
                AND perm.nPermLevel = 1
            )

            OR

            NOT EXISTS
            (
                SELECT 1
                FROM tblCartShippingPermission perm
                WHERE perm.nShippingMethodId = sm.nShipOptKey
                AND perm.nPermLevel = 1
            );

    END

    ------------------------------------------------------------
    -- BASE METHODS
    ------------------------------------------------------------

SELECT DISTINCT
    opt.nShipOptKey,
    opt.cCurrency,
    opt.cShipOptName,
    opt.cShipOptForeignRef,
    opt.cShipOptCarrier,
    opt.cShipOptTime,
    CAST(opt.cShipOptTandC AS NVARCHAR(MAX)) AS cShipOptTandC,
    opt.nShipOptCost,
    opt.nShipOptPercentage,
    opt.nShipOptQuantMin,
    opt.nShipOptQuantMax,
    opt.nShipOptWeightMin,
    opt.nShipOptWeightMax,
    opt.nShipOptPriceMin,
    opt.nShipOptPriceMax,
    opt.nShipOptHandlingPercentage,
    opt.nShipOptHandlingFixedCost,
    opt.nShipOptTaxRate,
    opt.nAuditId,
    opt.nDisplayPriority,
    opt.bCollection,
    opt.nShipOptCat,
    CAST(opt.nShipOptCost AS FLOAT) AS nShippingTotal,
    CAST('' AS NVARCHAR(500)) AS nShippingGroup,
    opt.bOverrideForWholeOrder,
    opt.nShipOptWeightOverageUnit,
    opt.nShipOptWeightOverageRate,
    loc.cLocationNameShort
INTO #BaseMethods
    FROM tblCartShippingMethods opt
    INNER JOIN tblAudit aud
        ON aud.nAuditKey = opt.nAuditId
    LEFT JOIN tblCartShippingRelations rel
        ON rel.nShpOptId = opt.nShipOptKey
    LEFT JOIN tblCartShippingLocations loc
        ON loc.nLocationKey = rel.nShpLocId
    WHERE
        aud.nStatus > 0

        AND
        (
            opt.cCurrency IS NULL
            OR opt.cCurrency = ''
            OR opt.cCurrency = @Currency
        )

        AND
        (
            aud.dPublishDate IS NULL
            OR aud.dPublishDate <= @dValidDate
        )

        AND
        (
            aud.dExpireDate IS NULL
            OR aud.dExpireDate >= @dValidDate
        )
        
AND
    (
        @ProductPrice <= 0

        OR

        (
            (
                opt.nShipOptPriceMin IS NULL
                OR opt.nShipOptPriceMin = 0
                OR opt.nShipOptPriceMin <= @ProductPrice
            )

            AND

            (
                opt.nShipOptPriceMax IS NULL
                OR opt.nShipOptPriceMax = 0
                OR opt.nShipOptPriceMax >= @ProductPrice
            )
        )
    )
        
        ;

    ------------------------------------------------------------
    -- FINAL METHODS
    ------------------------------------------------------------

    SELECT *
    
,CASE
    WHEN EXISTS
    (
        SELECT 1
        FROM #ShippingGroups sg
        WHERE sg.nShipOptId = bm.nShipOptKey
    )
    THEN 1
    ELSE 0
 END AS IsProductShippingGroupMethod

,CASE
    WHEN EXISTS
    (
        SELECT 1
        FROM #GroupRestrictedMethods gr
        WHERE gr.nShipOptId = bm.nShipOptKey
    )
    THEN 1
    ELSE 0
 END AS IsAnyShippingGroupMethod



    INTO #FinalMethods
    FROM #BaseMethods bm
    WHERE

        EXISTS
        (
            SELECT 1
            FROM #AllowedMethods am
            WHERE am.nShipOptId = bm.nShipOptKey
        )

        
AND
(
    bm.bCollection = 1

    OR

    NOT EXISTS
    (
        SELECT 1
        FROM #CountryFilter
    )

    OR

    EXISTS
    (
        SELECT 1
        FROM #CountryFilter cf
        WHERE cf.CountryCode = bm.cLocationNameShort
    )
)


       
AND
(
    (
        @HasShippingGroup = 1
        AND
        (
            bm.bCollection = 1

            OR EXISTS
            (
                SELECT 1
                FROM #ShippingGroups sg
                WHERE sg.nShipOptId = bm.nShipOptKey
            )
        )
    )

    OR

    (
        @HasShippingGroup = 0
        AND
        (
            bm.bCollection = 1

            OR NOT EXISTS
            (
                SELECT 1
                FROM #GroupRestrictedMethods gr
                WHERE gr.nShipOptId = bm.nShipOptKey
            )
        )
    )
)
;

    ------------------------------------------------------------
    -- DEBUG OUTPUTS
    ------------------------------------------------------------
    
IF @Debug = 1
BEGIN

    SELECT 'CountryFilter' AS DebugSet, *
    FROM #CountryFilter;

    SELECT 'AllowedMethods' AS DebugSet, *
    FROM #AllowedMethods;

    SELECT 'ShippingGroups' AS DebugSet, *
    FROM #ShippingGroups;

    SELECT 'GroupRestrictedMethods' AS DebugSet, *
    FROM #GroupRestrictedMethods;

    SELECT 'BaseMethods' AS DebugSet, *
    FROM #BaseMethods;
 END;

;WITH RankedMethods AS
(
    SELECT
        *,
        MAX(
            CASE
                WHEN ISNULL(bCollection,0) = 0
                THEN CAST(ISNULL(bOverrideForWholeOrder,0) AS INT)
                ELSE 0
            END
        ) OVER() AS _hasOverride,

        ROW_NUMBER() OVER
        (
            PARTITION BY nShipOptKey
            ORDER BY
                nDisplayPriority,
                nShippingTotal,
                cLocationNameShort
        ) AS _rn
    FROM #FinalMethods
)
SELECT
    nShipOptKey,
    cCurrency,
    cShipOptName,
    cShipOptForeignRef,
    cShipOptCarrier,
    cShipOptTime,
    cShipOptTandC,
    nShipOptCost,
    nShipOptPercentage,
    nShipOptQuantMin,
    nShipOptQuantMax,
    nShipOptWeightMin,
    nShipOptWeightMax,
    nShipOptPriceMin,
    nShipOptPriceMax,
    nShipOptHandlingPercentage,
    nShipOptHandlingFixedCost,
    nShipOptTaxRate,
    nAuditId,
    nDisplayPriority,
    bCollection,
    nShipOptCat,
    nShippingTotal,
    nShippingGroup,
    bOverrideForWholeOrder,
    nShipOptWeightOverageUnit,
    nShipOptWeightOverageRate,
    cLocationNameShort
FROM RankedMethods
WHERE
    _rn = 1
    AND
    (
        bCollection = 1
        OR _hasOverride = 0
        OR bOverrideForWholeOrder = 1
    )
ORDER BY
    nDisplayPriority,
    nShippingTotal;
END
GO