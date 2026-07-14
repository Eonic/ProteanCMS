CREATE OR ALTER PROCEDURE dbo.spGetValidShippingOptions_v2_Debug
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
    @ProductId        BIGINT,
    @GroupType        NVARCHAR(100) = 'Shipping',
    @Debug            BIT = 0
)
AS
BEGIN

    SET NOCOUNT ON;

    --------------------------------------------------------------------------
    -- DEFAULT DATE
    --------------------------------------------------------------------------

    IF @dValidDate IS NULL
        SET @dValidDate = GETDATE();

    --------------------------------------------------------------------------
    -- STAGE 1 : CART TOTALS
    --------------------------------------------------------------------------

    CREATE TABLE #CartTotals
    (
        Amount      FLOAT,
        Quantity    BIGINT,
        Weight      FLOAT
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
        CountryCode NVARCHAR(100) PRIMARY KEY
    );

    IF NULLIF(@CountryList,'') IS NOT NULL
    BEGIN

        INSERT INTO #CountryFilter
        (
            CountryCode
        )
        SELECT
            LTRIM(RTRIM(REPLACE(value,'''','')))
        FROM STRING_SPLIT(@CountryList, ',');

    END

    --------------------------------------------------------------------------
    -- STAGE 3 : SHIPPING GROUPS
    --------------------------------------------------------------------------

    CREATE TABLE #ShippingGroupList
    (
        ContentId BIGINT,
        nCatKey BIGINT,
        cCatName NVARCHAR(250),
        cCatSchemaName NVARCHAR(250)
    );

    CREATE TABLE #ShippingGroupMethods
    (
        nShipOptId BIGINT,
        nCatId BIGINT
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
-- BUILD SHIPPING GROUP LIST
--------------------------------------------------------------------------

IF @CartOrderId > 0
BEGIN

    INSERT INTO #ShippingGroupList
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



    INSERT INTO #ShippingGroupMethods
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
    FROM #ShippingGroupList SGL
    WHERE SGL.cCatSchemaName = @GroupType
      AND SGL.nCatKey = CSPC.nCatId
);


    
SELECT TOP (1)
    @ShippingGroupName = SGL.cCatName
FROM #ShippingGroupList SGL
WHERE SGL.cCatSchemaName = @GroupType
AND EXISTS
(
    SELECT 1
    FROM #ShippingGroupMethods SGM
    WHERE SGM.nCatId = SGL.nCatKey
)
ORDER BY
    SGL.nCatKey DESC;


--------------------------------------------------------------------------
-- GROUP COUNTS
--------------------------------------------------------------------------

SELECT
    @ExistShippingGroupCount = COUNT(*)
FROM #ShippingGroupList
WHERE cCatSchemaName = @GroupType;

SELECT
    @GroupItemCount = COUNT(DISTINCT ContentId)
FROM #ShippingGroupList
WHERE cCatSchemaName = @GroupType;

    --------------------------------------------------------------------------
    -- STAGE 4 : PERMISSIONS
    --------------------------------------------------------------------------

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

    --------------------------------------------------------------------------
    -- TODO
    -- Populate permission diagnostics here
    --------------------------------------------------------------------------

    UPDATE P
SET PermissionMatch =
CASE

    WHEN @UserId = 0 THEN
        CASE
            WHEN HasNonAuthAllow = 1
              OR HasAnyAllowRule = 0
            THEN 1
            ELSE 0
        END

    ELSE
        CASE
            WHEN HasUserDeny = 1
                THEN 0

            WHEN HasUserAllow = 1
                THEN 1

            WHEN HasAuthAllow = 1
                THEN 1

            WHEN HasAnyAllowRule = 0
                THEN 1

            ELSE 0
        END

END
FROM #PermissionStatus P;

    --------------------------------------------------------------------------
    -- STAGE 5 : BASE METHODS
    --------------------------------------------------------------------------

    SELECT
        opt.nShipOptKey,
        opt.cCurrency,
        opt.cShipOptName,
        opt.cShipOptForeignRef,
        opt.cShipOptCarrier,
        opt.cShipOptTime,
        CAST(opt.cShipOptTandC AS NVARCHAR(MAX)) cShipOptTandC,

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

        dbo.fxn_shippingTotal(
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

        loc.cLocationNameShort

    INTO #BaseMethods

    FROM tblCartShippingMethods opt
        INNER JOIN tblAudit aud
            ON opt.nAuditId = aud.nAuditKey
        LEFT JOIN tblCartShippingRelations rel
            ON rel.nShpOptId = opt.nShipOptKey
        LEFT JOIN tblCartShippingLocations loc
            ON loc.nLocationKey = rel.nShpLocId;

    --------------------------------------------------------------------------
    -- STAGE 6 : RULE DIAGNOSTICS
    --------------------------------------------------------------------------

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

    --------------------------------------------------------------------------
    -- TODO
    -- Populate evaluation table
    -- One row per method
    --------------------------------------------------------------------------

    --------------------------------------------------------------------------
    -- STAGE 7 : FINAL RESULTS
    --------------------------------------------------------------------------

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

    --------------------------------------------------------------------------
    -- TODO
    -- Insert matched methods here
    -- Apply:
    --   shipping groups
    --   override methods
    --   deduplication
    --   promo free shipping
    --------------------------------------------------------------------------

    --------------------------------------------------------------------------
    -- DEBUG OUTPUTS
    --------------------------------------------------------------------------

    IF @Debug = 1
    BEGIN

        SELECT * FROM #CartTotals;

        SELECT
            @CartItemCount AS CartItemCount,
            @GroupItemCount AS GroupItemCount,
            @ExistShippingGroupCount AS ExistShippingGroupCount,
            @ShippingGroupName AS ShippingGroupName;

        SELECT * FROM #CountryFilter;

        SELECT * FROM #ShippingGroupList;

        SELECT * FROM #ShippingGroupMethods;

        SELECT * FROM #PermissionStatus;

        SELECT * FROM #MethodEvaluation;

    END

    --------------------------------------------------------------------------
    -- FINAL OUTPUT (MUST MATCH ORIGINAL PROC)
    --------------------------------------------------------------------------

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
    FROM #FinalMethods
    ORDER BY
        nDisplayPriority,
        nShippingTotal;

END
GO