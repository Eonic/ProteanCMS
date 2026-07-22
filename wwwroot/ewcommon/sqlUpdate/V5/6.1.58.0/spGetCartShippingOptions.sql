CREATE OR ALTER PROCEDURE spGetCartShippingOptions
(
    @CartOrderId      BIGINT,
    @Amount           FLOAT = NULL,
    @Quantity         BIGINT = NULL,
    @Weight           FLOAT = NULL,
    @Currency         NVARCHAR(3),
    @UserId           BIGINT,
    @AuthUsers        BIGINT,
    @NonAuthUsers     BIGINT,
    @CountryList      NVARCHAR(1000),
    @dValidDate       DATE = NULL,
    @PromoCode        NVARCHAR(255),
    @GroupType        NVARCHAR(100) = 'Shipping',
    @Debug            BIT = 0
)
AS
BEGIN

    SET NOCOUNT ON;

    IF @dValidDate IS NULL
        SET @dValidDate = GETDATE();

    --------------------------------------------------------------------------
    -- STAGE 1 : CART TOTALS
    --------------------------------------------------------------------------

    CREATE TABLE #CartTotals
    (
        Amount FLOAT,
        Quantity BIGINT,
        Weight FLOAT
    );

    IF @CartOrderId > 0
    BEGIN

        SELECT
            @Amount =
                ISNULL(
                    CASE
                        WHEN @Amount IS NULL OR @Amount = 0
                        THEN SUM(nPrice * nQuantity)
                        ELSE @Amount
                    END,
                0),

            @Quantity =
                ISNULL(
                    CASE
                        WHEN @Quantity IS NULL OR @Quantity = 0
                        THEN SUM(nQuantity)
                        ELSE @Quantity
                    END,
                0),

            @Weight =
                ISNULL(
                    CASE
                        WHEN @Weight IS NULL OR @Weight = 0
                        THEN SUM(nWeight * nQuantity)
                        ELSE @Weight
                    END,
                0)

        FROM tblCartItem
        WHERE nCartOrderId = @CartOrderId
        AND nParentId = 0;

    END

    INSERT INTO #CartTotals
    VALUES
    (
        ISNULL(@Amount,0),
        ISNULL(@Quantity,0),
        ISNULL(@Weight,0)
    );

    --------------------------------------------------------------------------
    -- STAGE 2 : COUNTRY FILTER
    --------------------------------------------------------------------------

    CREATE TABLE #CountryFilter
    (
          CountryCode NVARCHAR(100) PRIMARY KEY,
          SortOrder INT
    );

    IF NULLIF(@CountryList,'') IS NOT NULL
    BEGIN

-- Use enable_ordinal (1) so SortOrder reflects the actual position of each
-- country code in @CountryList, since STRING_SPLIT does not otherwise
-- guarantee row order.
;WITH CountryList AS
(
    SELECT
        LTRIM(RTRIM(REPLACE(value,'''',''))) AS CountryCode,
        ROW_NUMBER() OVER (ORDER BY [ordinal]) AS SortOrder
    FROM STRING_SPLIT(@CountryList, ',', 1)
)
INSERT INTO #CountryFilter
(
    CountryCode,
    SortOrder
)
SELECT
    CountryCode,
    SortOrder
FROM CountryList
WHERE NULLIF(CountryCode,'') IS NOT NULL;

    END;

    --------------------------------------------------------------------------
    -- STAGE 3 : CART SHIPPING GROUPS
    --------------------------------------------------------------------------

    CREATE TABLE #CartShippingGroups
    (
        ContentId BIGINT,
        nCatKey BIGINT,
        cCatName NVARCHAR(250),
        cCatSchemaName NVARCHAR(250)
    );

      --------------------------------------------------------------------------
    -- BUILD SHIPPING GROUP LIST
    --------------------------------------------------------------------------

    IF @CartOrderId > 0
    BEGIN

        INSERT INTO #CartShippingGroups
        (
            ContentId,
            nCatKey,
            cCatName,
            cCatSchemaName
        )
        SELECT
            i.nItemId,
            cpc.nCatKey,
            cpc.cCatName,
            cpc.cCatSchemaName
        FROM tblCartItem i
        INNER JOIN tblContent p
            ON i.nItemId = p.nContentKey
        LEFT JOIN tblCartCatProductRelations cpr
            ON p.nContentKey = cpr.nContentId
        LEFT JOIN tblCartProductCategories cpc
            ON cpr.nCatId = cpc.nCatKey
        WHERE i.nCartOrderId = @CartOrderId
          AND i.nItemId <> 0

        UNION

        SELECT
            cr.nContentParentId,
            cpc.nCatKey,
            cpc.cCatName,
            cpc.cCatSchemaName
        FROM tblCartItem i
        INNER JOIN tblContent p
            ON i.nItemId = p.nContentKey
        INNER JOIN tblContentRelation cr
            ON i.nItemId = cr.nContentChildId
        LEFT JOIN tblCartCatProductRelations cpr
            ON cr.nContentParentId = cpr.nContentId
        LEFT JOIN tblCartProductCategories cpc
            ON cpr.nCatId = cpc.nCatKey
        WHERE i.nCartOrderId = @CartOrderId
          AND i.nItemId <> 0
          AND p.cContentSchemaName = 'SKU';

    END;


    CREATE TABLE #CartShippingGroupMethods
    (
        nShipOptId BIGINT,
        nCatId BIGINT
    );

    -- Tracks methods explicitly denied (nRuleType = 0) for a matched category,
    -- so an explicit deny can override an allow rule from another category.
    CREATE TABLE #CartShippingGroupDenyMethods
    (
        nShipOptId BIGINT,
        nCatId BIGINT
    );

    CREATE TABLE #CartShippingMethods
    (
        nShipOptId BIGINT PRIMARY KEY
    );

    CREATE TABLE #CartShippingDeniedMethods
    (
        nShipOptId BIGINT PRIMARY KEY
    );

    -- All shipping options restricted (nRuleType = 1) to ANY category of
    -- @GroupType, regardless of whether that category is present in the cart.
    -- Needed to correctly distinguish "unrestricted method" from
    -- "restricted method whose required category isn't in this cart".
    CREATE TABLE #AllGroupRestrictedMethods
    (
        nShipOptId BIGINT PRIMARY KEY
    );

    DECLARE @CartItemCount INT = 0;
    DECLARE @GroupItemCount INT = 0;
    DECLARE @ExistShippingGroupCount INT = 0;
    DECLARE @ShippingGroupName NVARCHAR(500) = '';

    --------------------------------------------------------------------------
    -- CART ITEM COUNT
    --------------------------------------------------------------------------

    SELECT
        @CartItemCount = COUNT(*)
    FROM tblCartItem
    WHERE nCartOrderId = @CartOrderId
    AND nParentId = 0;

  

    --------------------------------------------------------------------------
    -- GROUP COUNTS
    --------------------------------------------------------------------------

    SELECT
        @ExistShippingGroupCount = COUNT(*)
    FROM #CartShippingGroups
    WHERE cCatSchemaName = @GroupType;

    SELECT
        @GroupItemCount = COUNT(DISTINCT ContentId)
    FROM #CartShippingGroups
    WHERE cCatSchemaName = @GroupType;

    --------------------------------------------------------------------------
    -- SHIPPING GROUP METHODS
    --------------------------------------------------------------------------

    INSERT INTO #CartShippingGroupMethods
    (
        nShipOptId,
        nCatId
    )
    SELECT DISTINCT
        CSPC.nShipOptId,
        CSPC.nCatId
    FROM tblCartShippingProductCategoryRelations CSPC
    WHERE CSPC.nRuleType = 1
    AND EXISTS
    (
        SELECT 1
        FROM #CartShippingGroups CSG
        WHERE CSG.cCatSchemaName = @GroupType
          AND CSG.nCatKey = CSPC.nCatId
    );

    INSERT INTO #CartShippingMethods
    (
        nShipOptId
    )
    SELECT DISTINCT
        nShipOptId
    FROM #CartShippingGroupMethods;

    --------------------------------------------------------------------------
    -- ALL METHODS RESTRICTED TO ANY CATEGORY OF @GroupType
    -- (used to detect methods that require a group the cart does not have)
    --------------------------------------------------------------------------

    INSERT INTO #AllGroupRestrictedMethods
    (
        nShipOptId
    )
    SELECT DISTINCT
        CSPC.nShipOptId
    FROM tblCartShippingProductCategoryRelations CSPC
    INNER JOIN tblCartProductCategories cpc
        ON cpc.nCatKey = CSPC.nCatId
    WHERE CSPC.nRuleType = 1
      AND cpc.cCatSchemaName = @GroupType;

    --------------------------------------------------------------------------
    -- SHIPPING GROUP DENY METHODS
    -- (nRuleType = 0 rows explicitly deny a shipping method for a category)
    --------------------------------------------------------------------------

    INSERT INTO #CartShippingGroupDenyMethods
    (
        nShipOptId,
        nCatId
    )
    SELECT DISTINCT
        CSPC.nShipOptId,
        CSPC.nCatId
    FROM tblCartShippingProductCategoryRelations CSPC
    WHERE CSPC.nRuleType = 0
    AND EXISTS
    (
        SELECT 1
        FROM #CartShippingGroups CSG
        WHERE CSG.cCatSchemaName = @GroupType
          AND CSG.nCatKey = CSPC.nCatId
    );

    INSERT INTO #CartShippingDeniedMethods
    (
        nShipOptId
    )
    SELECT DISTINCT
        nShipOptId
    FROM #CartShippingGroupDenyMethods;

    --------------------------------------------------------------------------
    -- SHIPPING GROUP NAME
    --------------------------------------------------------------------------

    SELECT TOP (1)
        @ShippingGroupName = CSG.cCatName
    FROM #CartShippingGroups CSG
    WHERE CSG.cCatSchemaName = @GroupType
      AND EXISTS
      (
          SELECT 1
          FROM #CartShippingGroupMethods CSGM
          WHERE CSGM.nCatId = CSG.nCatKey
      )
    ORDER BY
        CSG.nCatKey DESC;


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

    dbo.fxn_shippingTotal
    (
        opt.nShipOptKey,
        @Amount,
        @Quantity,
        @Weight
    ) AS nShippingTotal,

    CAST(0.00 AS FLOAT) AS NonDiscountedShippingCost,

    CAST('' AS NVARCHAR(500)) AS nShippingGroup,

    opt.bOverrideForWholeOrder,

    opt.nShipOptWeightOverageUnit,
    opt.nShipOptWeightOverageRate,

    aud.nStatus,
    aud.dPublishDate,
    aud.dExpireDate,

    loc.cLocationNameShort

INTO #BaseMethods

FROM tblCartShippingMethods opt
INNER JOIN tblAudit aud
    ON opt.nAuditId = aud.nAuditKey

LEFT JOIN tblCartShippingRelations rel
    ON rel.nShpOptId = opt.nShipOptKey

LEFT JOIN tblCartShippingLocations loc
    ON loc.nLocationKey = rel.nShpLocId;

CREATE TABLE #PermissionStatus
(
    nShipOptId BIGINT PRIMARY KEY,

    HasAnyAllowRule BIT,
    HasAuthAllow BIT,
    HasNonAuthAllow BIT,

    HasUserAllow BIT,
    HasUserDeny BIT,

    PermissionMatch BIT
);

INSERT INTO #PermissionStatus
(
    nShipOptId,
    HasAnyAllowRule,
    HasAuthAllow,
    HasNonAuthAllow,
    HasUserAllow,
    HasUserDeny,
    PermissionMatch
)
SELECT
    sm.nShipOptKey,

    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM tblCartShippingPermission p
            WHERE p.nShippingMethodId = sm.nShipOptKey
              AND p.nPermLevel = 1
        )
        THEN 1 ELSE 0
    END,

    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM tblCartShippingPermission p
            WHERE p.nShippingMethodId = sm.nShipOptKey
              AND p.nDirId = @AuthUsers
              AND p.nPermLevel = 1
        )
        THEN 1 ELSE 0
    END,

    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM tblCartShippingPermission p
            WHERE p.nShippingMethodId = sm.nShipOptKey
              AND p.nDirId = @NonAuthUsers
              AND p.nPermLevel = 1
        )
        THEN 1 ELSE 0
    END,

    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM tblCartShippingPermission p
            INNER JOIN tblDirectoryRelation dr
                ON dr.nDirParentId = p.nDirId
            WHERE p.nShippingMethodId = sm.nShipOptKey
              AND dr.nDirChildId = @UserId
              AND p.nPermLevel = 1
        )
        THEN 1 ELSE 0
    END,

    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM tblCartShippingPermission p
            INNER JOIN tblDirectoryRelation dr
                ON dr.nDirParentId = p.nDirId
            WHERE p.nShippingMethodId = sm.nShipOptKey
              AND dr.nDirChildId = @UserId
              AND p.nPermLevel = 0
        )
        THEN 1 ELSE 0
    END,

    0
FROM tblCartShippingMethods sm;


UPDATE p
SET PermissionMatch =
CASE
    WHEN @UserId = 0
    THEN
        CASE
            WHEN HasNonAuthAllow = 1
              OR HasAnyAllowRule = 0
            THEN 1
            ELSE 0
        END
    ELSE
        CASE
            WHEN HasUserDeny = 1 THEN 0
            WHEN HasUserAllow = 1 THEN 1
            WHEN HasAuthAllow = 1 THEN 1
            WHEN HasAnyAllowRule = 0 THEN 1
            ELSE 0
        END
END
FROM #PermissionStatus p;


CREATE TABLE #MethodEvaluation
(
    nShipOptId BIGINT,

    QuantityMatch BIT,
    PriceMatch BIT,
    WeightMatch BIT,

    CurrencyMatch BIT,
    PermissionMatch BIT,
    CountryMatch BIT,
    ShippingGroupMatch BIT,

    AuditMatch BIT,

    OverrideMethod BIT,

    FinalMatch BIT
);

INSERT INTO #MethodEvaluation
(
    nShipOptId,
    QuantityMatch,
    PriceMatch,
    WeightMatch,
    CurrencyMatch,
    PermissionMatch,
    CountryMatch,
    ShippingGroupMatch,
    AuditMatch,
    OverrideMethod,
    FinalMatch
)
SELECT
    bm.nShipOptKey,

    ----------------------------------------------------------------------
    -- QUANTITY
    ----------------------------------------------------------------------

    CASE
        WHEN
            (bm.nShipOptQuantMin <= 0 OR bm.nShipOptQuantMin <= @Quantity)
        AND
            (bm.nShipOptQuantMax <= 0 OR bm.nShipOptQuantMax >= @Quantity)
        THEN 1
        ELSE 0
    END,

    ----------------------------------------------------------------------
    -- PRICE
    ----------------------------------------------------------------------

    CASE
        WHEN
            (bm.nShipOptPriceMin <= 0 OR bm.nShipOptPriceMin <= @Amount)
        AND
            (bm.nShipOptPriceMax <= 0 OR bm.nShipOptPriceMax >= @Amount)
        THEN 1
        ELSE 0
    END,

    ----------------------------------------------------------------------
    -- WEIGHT
    ----------------------------------------------------------------------

    CASE
        WHEN
            (bm.nShipOptWeightMin <= 0 OR bm.nShipOptWeightMin <= @Weight)
        AND
            (bm.nShipOptWeightMax <= 0 OR bm.nShipOptWeightMax >= @Weight)
        THEN 1
        ELSE 0
    END,

    ----------------------------------------------------------------------
    -- CURRENCY
    ----------------------------------------------------------------------

    CASE
        WHEN
            bm.cCurrency IS NULL
            OR bm.cCurrency = ''
            OR bm.cCurrency = @Currency
        THEN 1
        ELSE 0
    END,

    ----------------------------------------------------------------------
    -- PERMISSIONS
    ----------------------------------------------------------------------

    CASE
        WHEN bm.bCollection = 1
            THEN 1

        WHEN ps.PermissionMatch = 1
            THEN 1

        ELSE 0
    END,

    ----------------------------------------------------------------------
    -- COUNTRY
    ----------------------------------------------------------------------

    CASE
        WHEN bm.bCollection = 1
            THEN 1

        WHEN NOT EXISTS
        (
            SELECT 1
            FROM #CountryFilter
        )
            THEN 1

        WHEN EXISTS
        (
            SELECT 1
            FROM #CountryFilter cf
            WHERE cf.CountryCode = bm.cLocationNameShort
        )
            THEN 1

        ELSE 0
    END,

    ----------------------------------------------------------------------
    -- SHIPPING GROUP
    ----------------------------------------------------------------------

    0,

----------------------------------------------------------------------
-- AUDIT
----------------------------------------------------------------------

CASE
    WHEN
        bm.nStatus > 0

        AND
        (
            bm.dPublishDate IS NULL
            OR bm.dPublishDate <= @dValidDate
        )

        AND
        (
            bm.dExpireDate IS NULL
            OR bm.dExpireDate >= @dValidDate
        )

    THEN 1

    ELSE 0
END,

    ----------------------------------------------------------------------
    -- OVERRIDE
    ----------------------------------------------------------------------

    CASE
        WHEN ISNULL(bm.bOverrideForWholeOrder,0) = 1
        THEN 1
        ELSE 0
    END,

    ----------------------------------------------------------------------
    -- FINAL MATCH
    ----------------------------------------------------------------------

    0

FROM #BaseMethods bm

LEFT JOIN #PermissionStatus ps
    ON ps.nShipOptId = bm.nShipOptKey;

    UPDATE me
SET ShippingGroupMatch =
CASE

    WHEN b.bCollection = 1
        THEN 1

    -- Explicit deny always wins over any allow rule for the shipping group.
    WHEN EXISTS
    (
        SELECT 1
        FROM #CartShippingDeniedMethods csdm
        WHERE csdm.nShipOptId = me.nShipOptId
    )
        THEN 0

    WHEN @ExistShippingGroupCount > 0
    THEN
        CASE
            WHEN EXISTS
            (
                SELECT 1
                FROM #CartShippingMethods csm
                WHERE csm.nShipOptId = me.nShipOptId
            )
            THEN 1
            ELSE 0
        END

    -- Cart has no item in @GroupType category. A method is only unrestricted
    -- (and therefore allowed) if it is NOT tied to any @GroupType category at
    -- all. If it requires a specific category that the cart doesn't have,
    -- it must be excluded rather than defaulting to allowed.
    ELSE
        CASE
            WHEN EXISTS
            (
                SELECT 1
                FROM #AllGroupRestrictedMethods agrm
                WHERE agrm.nShipOptId = me.nShipOptId
            )
            THEN 0
            ELSE 1
        END

END
FROM #MethodEvaluation me
INNER JOIN #BaseMethods b
    ON b.nShipOptKey = me.nShipOptId;


UPDATE me
SET FinalMatch =
CASE
    WHEN
        QuantityMatch = 1
        AND PriceMatch = 1
        AND WeightMatch = 1
        AND CurrencyMatch = 1
        AND PermissionMatch = 1
        AND CountryMatch = 1
        AND ShippingGroupMatch = 1
        AND AuditMatch = 1
    THEN 1
    ELSE 0
END
FROM #MethodEvaluation me;





 CREATE TABLE #FinalMethods
(
    nShipOptKey INT,

    cCurrency NVARCHAR(10),

    cShipOptName NVARCHAR(255),
    cShipOptForeignRef NVARCHAR(255),
    cShipOptCarrier NVARCHAR(100),
    cShipOptTime NVARCHAR(50),
    cShipOptTandC NVARCHAR(MAX),

    nShipOptCost MONEY,
    nShipOptPercentage FLOAT,

    nShipOptQuantMin FLOAT,
    nShipOptQuantMax FLOAT,

    nShipOptWeightMin FLOAT,
    nShipOptWeightMax FLOAT,

    nShipOptPriceMin FLOAT,
    nShipOptPriceMax FLOAT,

    nShipOptHandlingPercentage FLOAT,
    nShipOptHandlingFixedCost FLOAT,
    nShipOptTaxRate FLOAT,

    nAuditId INT,
    nDisplayPriority INT,

    bCollection BIT,
    nShipOptCat INT,

    nShippingTotal FLOAT,
    NonDiscountedShippingCost FLOAT,

    nShippingGroup NVARCHAR(500),

    bOverrideForWholeOrder INT,

    nShipOptWeightOverageUnit FLOAT,
    nShipOptWeightOverageRate FLOAT,

    cLocationNameShort NVARCHAR(100)
);

 INSERT INTO #FinalMethods
(
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
    NonDiscountedShippingCost,
    nShippingGroup,
    bOverrideForWholeOrder,
    nShipOptWeightOverageUnit,
    nShipOptWeightOverageRate,
    cLocationNameShort
)
SELECT
    bm.nShipOptKey,
    bm.cCurrency,
    bm.cShipOptName,
    bm.cShipOptForeignRef,
    bm.cShipOptCarrier,
    bm.cShipOptTime,
    bm.cShipOptTandC,
    bm.nShipOptCost,
    bm.nShipOptPercentage,
    bm.nShipOptQuantMin,
    bm.nShipOptQuantMax,
    bm.nShipOptWeightMin,
    bm.nShipOptWeightMax,
    bm.nShipOptPriceMin,
    bm.nShipOptPriceMax,
    bm.nShipOptHandlingPercentage,
    bm.nShipOptHandlingFixedCost,
    bm.nShipOptTaxRate,
    bm.nAuditId,
    bm.nDisplayPriority,
    bm.bCollection,
    bm.nShipOptCat,
    bm.nShippingTotal,
    bm.NonDiscountedShippingCost,
    bm.nShippingGroup,
    bm.bOverrideForWholeOrder,
    bm.nShipOptWeightOverageUnit,
    bm.nShipOptWeightOverageRate,
    bm.cLocationNameShort
FROM #BaseMethods bm
INNER JOIN #MethodEvaluation me
    ON me.nShipOptId = bm.nShipOptKey
WHERE me.FinalMatch = 1;

CREATE TABLE #FinalMethodsDeduped
(
    nShipOptKey INT,

    cCurrency NVARCHAR(10),

    cShipOptName NVARCHAR(255),
    cShipOptForeignRef NVARCHAR(255),
    cShipOptCarrier NVARCHAR(100),
    cShipOptTime NVARCHAR(50),
    cShipOptTandC NVARCHAR(MAX),

    nShipOptCost MONEY,
    nShipOptPercentage FLOAT,

    nShipOptQuantMin FLOAT,
    nShipOptQuantMax FLOAT,

    nShipOptWeightMin FLOAT,
    nShipOptWeightMax FLOAT,

    nShipOptPriceMin FLOAT,
    nShipOptPriceMax FLOAT,

    nShipOptHandlingPercentage FLOAT,
    nShipOptHandlingFixedCost FLOAT,
    nShipOptTaxRate FLOAT,

    nAuditId INT,
    nDisplayPriority INT,

    bCollection BIT,
    nShipOptCat INT,

    nShippingTotal FLOAT,
    NonDiscountedShippingCost FLOAT,

    nShippingGroup NVARCHAR(500),

    bOverrideForWholeOrder INT,

    nShipOptWeightOverageUnit FLOAT,
    nShipOptWeightOverageRate FLOAT,

    cLocationNameShort NVARCHAR(100)
);

;WITH Ranked AS
(
    SELECT
        fm.*,

        ROW_NUMBER() OVER
        (
            PARTITION BY fm.nShipOptKey
            ORDER BY
                ISNULL(cf.SortOrder,999999),
                fm.nDisplayPriority,
                fm.nShippingTotal,
                fm.cLocationNameShort
        ) AS rn

    FROM #FinalMethods fm

    LEFT JOIN #CountryFilter cf
        ON cf.CountryCode = fm.cLocationNameShort
)
INSERT INTO #FinalMethodsDeduped
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
    NonDiscountedShippingCost,
    nShippingGroup,
    bOverrideForWholeOrder,
    nShipOptWeightOverageUnit,
    nShipOptWeightOverageRate,
    cLocationNameShort
FROM Ranked
WHERE rn = 1;

DECLARE @HasOverride BIT = 0;

-- A method's bOverrideForWholeOrder=1 flag should only lock out all other
-- (non-collection) shipping methods when it genuinely applies to this cart's
-- contents — i.e. the method is linked (nRuleType = 1) to a @GroupType
-- category that at least one cart item actually belongs to
-- (#CartShippingMethods). Without this check, an override method that has no
-- real connection to the cart's items could incorrectly suppress every other
-- shipping option. Collection methods are exempt from the override entirely.
IF EXISTS
(
    SELECT 1
    FROM #FinalMethodsDeduped fmd
    WHERE ISNULL(fmd.bCollection,0) = 0
    AND ISNULL(fmd.bOverrideForWholeOrder,0) = 1
    AND EXISTS
    (
        SELECT 1
        FROM #CartShippingMethods csm
        WHERE csm.nShipOptId = fmd.nShipOptKey
    )
)
BEGIN
    SET @HasOverride = 1;
END

CREATE TABLE #FinalOutput
(
    nShipOptKey INT,
    cCurrency NVARCHAR(10),
    cShipOptName NVARCHAR(255),
    cShipOptForeignRef NVARCHAR(255),
    cShipOptCarrier NVARCHAR(100),
    cShipOptTime NVARCHAR(50),
    cShipOptTandC NVARCHAR(MAX),
    nShipOptCost MONEY,
    nShipOptPercentage FLOAT,
    nShipOptQuantMin FLOAT,
    nShipOptQuantMax FLOAT,
    nShipOptWeightMin FLOAT,
    nShipOptWeightMax FLOAT,
    nShipOptPriceMin FLOAT,
    nShipOptPriceMax FLOAT,
    nShipOptHandlingPercentage FLOAT,
    nShipOptHandlingFixedCost FLOAT,
    nShipOptTaxRate FLOAT,
    nAuditId INT,
    nDisplayPriority INT,
    bCollection BIT,
    nShipOptCat INT,
    nShippingTotal FLOAT,
    NonDiscountedShippingCost FLOAT,
    nShippingGroup NVARCHAR(500),
    bOverrideForWholeOrder INT,
    nShipOptWeightOverageUnit FLOAT,
    nShipOptWeightOverageRate FLOAT,
    cLocationNameShort NVARCHAR(100)
);

INSERT INTO #FinalOutput
SELECT fmd.*
FROM #FinalMethodsDeduped fmd
WHERE
(
    @HasOverride = 0
)
OR
(
    fmd.bCollection = 1
)
OR
(
    -- Only genuinely cart-linked override methods pass through once an
    -- override is active; a method flagged bOverrideForWholeOrder=1 but not
    -- tied to any item in this cart must not ride along.
    ISNULL(fmd.bOverrideForWholeOrder,0) = 1
    AND EXISTS
    (
        SELECT 1
        FROM #CartShippingMethods csm
        WHERE csm.nShipOptId = fmd.nShipOptKey
    )
);

--------------------------------------------------------------------------
-- FREE SHIPPING PROMO CODE
--------------------------------------------------------------------------

CREATE TABLE #FreeShippingMethods
(
    nShipOptId BIGINT PRIMARY KEY
);

IF @PromoCode IS NOT NULL
AND @PromoCode <> ''
BEGIN

    DECLARE @FreeShippingList NVARCHAR(MAX);

    SELECT
        @FreeShippingList =
            CONVERT(XML, cAdditionalXML)
                .value('(/cFreeShippingMethods)[1]', 'nvarchar(max)')
    FROM tblCartDiscountRules
    WHERE cDiscountCode = @PromoCode;

    IF NULLIF(@FreeShippingList,'') IS NOT NULL
    BEGIN

        INSERT INTO #FreeShippingMethods
        (
            nShipOptId
        )
        SELECT
            TRY_CAST(value AS BIGINT)
        FROM STRING_SPLIT(@FreeShippingList, ',')
        WHERE TRY_CAST(value AS BIGINT) IS NOT NULL;

        UPDATE fo
        SET
            NonDiscountedShippingCost = fo.nShipOptCost,
            nShipOptCost = 0,
            nShippingTotal = 0
        FROM #FinalOutput fo
        INNER JOIN #FreeShippingMethods fsm
            ON fsm.nShipOptId = fo.nShipOptKey;

    END

END;--------------------------------------------------------------------------
-- SHIPPING GROUP NAME
--------------------------------------------------------------------------

IF @ShippingGroupName <> ''
BEGIN

    UPDATE #FinalOutput
    SET nShippingGroup = @ShippingGroupName;

END;

IF @Debug = 1
BEGIN
        SELECT * from tblCartItem where nCartOrderId =     @CartOrderId 

        SELECT @CartItemCount;
        SELECT @GroupItemCount;
        SELECT @ExistShippingGroupCount;

        SELECT 'CartShippingGroups'
        SELECT *
        FROM #CartShippingGroups;

        SELECT *
        FROM #CartShippingGroupMethods;

        SELECT *
        FROM #CartShippingGroupDenyMethods;

        SELECT *
        FROM #AllGroupRestrictedMethods;


        SELECT TOP 10 *
        FROM #PermissionStatus;

        SELECT 'BaseMethods'
        SELECT *
        FROM #BaseMethods;
        
        SELECT 'MethodEvaluation'
        SELECT *
        FROM #MethodEvaluation;

        SELECT 'FinalMethodsDeduped'
        SELECT *
        FROM #FinalMethodsDeduped
 END;

SELECT
    *
FROM #FinalOutput

END 
GO