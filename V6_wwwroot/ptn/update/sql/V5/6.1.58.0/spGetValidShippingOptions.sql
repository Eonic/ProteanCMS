
 ALTER PROCEDURE [dbo].[spGetValidShippingOptions]
 -- Returns the valid shipping options available for a given cart order.
 -- Shipping options are filtered by quantity, price, weight, currency, country, CMS audit status,
 -- and user permissions. Results can be further restricted to options associated with product
 -- shipping group categories. If a promo code granting free shipping is supplied, the cost of
 -- those methods is zeroed out in the returned result set, with the original cost preserved.
 @CartOrderId      BIGINT,            -- ID of the cart order being processed
 @Amount           FLOAT      = NULL, -- Total monetary value of the cart (SUM of nPrice*nQuantity); derived from tblCartItem when NULL or 0 and @CartOrderId is supplied
 @Quantity         BIGINT     = NULL, -- Total item quantity in the cart (SUM of nQuantity); derived from tblCartItem when NULL or 0 and @CartOrderId is supplied
 @Weight           FLOAT      = NULL, -- Total weight of the cart (SUM of nWeight*nQuantity); derived from tblCartItem when NULL or 0 and @CartOrderId is supplied
 @Currency         NVARCHAR(3),       -- 3-character currency code (e.g. 'GBP'); options must match or have no currency restriction
 @userId           BIGINT,            -- Current user ID; 0 = anonymous/unauthenticated
 @AuthUsers        BIGINT,            -- Directory group ID representing all authenticated users (for permission checks)
 @NonAuthUsers     BIGINT,            -- Directory group ID representing unauthenticated/guest users (for permission checks)
 @CountryList      NVARCHAR(1000),    -- Comma-separated country codes/names to restrict shipping locations; empty = all countries
 @dValidDate       Date = GetDate,    -- Reference date for publish/expire date checks; defaults to today
 @PromoCode        NVARCHAR(255),     -- Promotional discount code; shipping methods it grants for free will have their cost zeroed
 @ProductId        BIGINT,            -- ID of a specific product (available for future contextual filtering)
 @GroupType        NVARCHAR(100) ='Shipping' -- Product category schema name identifying shipping groups (default = 'Shipping')
 
  
AS  
BEGIN  
-- SET NOCOUNT ON added to prevent extra result sets from  
 -- interfering with SELECT statements.  
 SET NOCOUNT ON;  

  -- -------------------------------------------------------------------------
  -- Derive @Amount, @Quantity, and @Weight from tblCartItem when any of them
  -- are not supplied by the caller and a valid @CartOrderId is present.
  -- Pass NULL or 0 for any of these parameters to have them derived.
  -- Only top-level items (nParentId = 0) are included, consistent with the
  -- @CartItemCount calculation later in this procedure.
  --   @Amount   = SUM(nPrice * nQuantity)  – total monetary value of the cart
  --   @Quantity = SUM(nQuantity)           – total number of units
  --   @Weight   = SUM(nWeight * nQuantity) – total weight across all items
  -- Individual values are only overwritten when they are still NULL or 0, so a
  -- caller may supply some values explicitly and leave others as NULL or 0.
  -- -------------------------------------------------------------------------
  IF @CartOrderId > 0 AND (@Amount IS NULL OR @Amount = 0 OR @Quantity IS NULL OR @Quantity = 0 OR @Weight IS NULL OR @Weight = 0)
  BEGIN
	  SELECT
		  @Amount   = ISNULL(CASE WHEN @Amount   IS NULL OR @Amount   = 0 THEN SUM(nPrice * nQuantity) ELSE @Amount   END, 0),
		  @Quantity = ISNULL(CASE WHEN @Quantity IS NULL OR @Quantity = 0 THEN SUM(nQuantity)           ELSE @Quantity END, 0),
		  @Weight   = ISNULL(CASE WHEN @Weight   IS NULL OR @Weight   = 0 THEN SUM(nWeight * nQuantity) ELSE @Weight   END, 0)
	  FROM tblCartItem
	  WHERE nCartOrderId = @CartOrderId
		AND nParentId = 0
  END

  -- Ensure we never pass NULL into the dynamic SQL fragments even when
  -- @CartOrderId was 0 or the cart has no items.
  SELECT
	  @Amount   = ISNULL(@Amount,   0),
	  @Quantity = ISNULL(@Quantity, 0),
	  @Weight   = ISNULL(@Weight,   0)

  -- Dynamic SQL string fragments assembled at runtime based on parameter values.
  -- The final query is constructed by concatenating these, then wrapping in a CTE (see below).
  Declare @strFirstQuery NVARCHAR(MAX) =''             -- SELECT and FROM/JOIN clause
  Declare @strSecondQuery NVARCHAR(MAX) =''            -- WHERE clause: range filters + user permission CASE check
  Declare @strEndConditionQuery NVARCHAR(MAX) =''      -- Additional WHERE: audit status + publish/expire dates; shipping group IN filter appended here when applicable
  Declare @strCountryConditionQuery NVARCHAR(MAX) =''  -- Optional AND clause to filter results by country
  Declare @shippingGroupCondition NVARCHAR(MAX) =''    -- Optional JOIN clause injected when ALL cart items belong to a shipping group
  Declare @shippingGroupRuleTypeCondition NVARCHAR(MAX) ='' -- AND clause enforcing nRuleType=1 on the shipping group join
  Declare @MainWhereCondition NVARCHAR(MAX) =''        -- Reserved/unused placeholder
  Declare @OrderByCondition NVARCHAR(MAX) =''          -- Reserved/unused placeholder (ORDER BY is embedded in the final CTE query)
  Declare @strMainQuery NVARCHAR(MAX) =''              -- The fully assembled dynamic SQL string, executed at the end of each branch

  -- Shipping group state and item counts
  Declare @ExistShippingGroupCount INT =0    -- Number of shipping group category records found for items in this order (> 0 means groups are active)
  Declare @CartItemCount As Int=0            -- Count of top-level cart items (nParentId = 0); excludes bundled/child items
  Declare @GroupItemCount As int=0           -- Count of cart items that belong to a shipping group category
  Declare @OverrideForWholeOrder NVARCHAR(MAX); -- Reserved placeholder; override logic is handled via the bOverrideForWholeOrder column in the CTE
  Declare @ShippingGroupName NVARCHAR(MAX) =''   -- Display name of the resolved shipping group, written into the nShippingGroup result column
  DECLARE @ShippingGroupCatIDList NVARCHAR(MAX); -- Comma-separated list of shipping group category IDs for embedding in dynamic SQL IN clauses


   -- -------------------------------------------------------------------------
   -- Build the SELECT/FROM dynamic SQL fragment.
   -- Joins shipping locations → location-method relations → shipping methods,
   -- then to tblAudit to support publish/expire filtering.
   -- fxn_shippingTotal computes the final delivery cost for the given method
   -- based on the order's amount, quantity, and weight (including overage rules).
   -- NonDiscountedShippingCost is initialised to 0.00 here; it is updated later
   -- if a promo code grants free shipping on a method.
   -- nShippingGroup is left blank here and populated after execution when a
   -- shipping group is in scope.
   -- -------------------------------------------------------------------------
	SET @strFirstQuery= 'select DISTINCT opt.nShipOptKey,opt.cCurrency,opt.cShipOptName,opt.cShipOptForeignRef,opt.cShipOptCarrier,opt.cShipOptTime, CAST(opt.cShipOptTandC AS NVARCHAR(MAX)) AS cShipOptTandC
,opt.nShipOptCost,opt.nShipOptPercentage, opt.nShipOptQuantMin, opt.nShipOptQuantMax, opt.nShipOptWeightMin, opt.nShipOptWeightMax,  
opt.nShipOptPriceMin, opt.nShipOptPriceMax, opt.nShipOptHandlingPercentage, opt.nShipOptHandlingFixedCost, opt.nShipOptTaxRate,  
opt.nAuditId, opt.nDisplayPriority, opt.bCollection, opt.nShipOptCat, dbo.fxn_shippingTotal(opt.nShipOptKey,'+convert(NVARCHAR(10), @Amount)+','+convert(NVARCHAR(10), @Quantity)+','+convert(NVARCHAR(10), @Weight)+') as nShippingTotal,
0.00 AS NonDiscountedShippingCost, '''' AS nShippingGroup,bOverrideForWholeOrder
,opt.nShipOptWeightOverageUnit
,opt.nShipOptWeightOverageRate
,Loc.cLocationNameShort  
from tblCartShippingLocations Loc   
   Inner Join tblCartShippingRelations rel ON Loc.nLocationKey = rel.nShpLocId   
   Inner Join tblCartShippingMethods opt ON rel.nShpOptId = opt.nShipOptKey   
   INNER JOIN tblAudit ON opt.nAuditId = tblAudit.nAuditKey '  
    
  -- -------------------------------------------------------------------------
  -- Build the WHERE dynamic SQL fragment.
  -- Range filters: quantity, price, and weight min/max (0 = no limit).
  -- Currency filter: accepts a NULL or empty currency (= applies to all) or
  --   must exactly match @Currency.
  -- User permission CASE:
  --   When @userId = 0 (anonymous): the method is included if the non-auth
  --     group (@NonAuthUsers) has been explicitly granted access (nPermLevel=1),
  --     or if no permission rows exist at all (open to everyone).
  --   When @userId > 0 (authenticated): the method is included if the user
  --     belongs to a group granted access AND is not in a group explicitly
  --     denied (nPermLevel=0), OR the global auth-users group is granted access,
  --     OR there are no permission restrictions at all. The method is then
  --     excluded if the user is in any explicitly denied group.
  -- -------------------------------------------------------------------------
  SET @strSecondQuery ='WHERE (nShipOptQuantMin <= 0 or nShipOptQuantMin <= '+convert(NVARCHAR(10), @Quantity)+') and (nShipOptQuantMax <= 0 or nShipOptQuantMax >= '+convert(NVARCHAR(10), @Quantity)+')
   and (nShipOptPriceMin <= 0 or nShipOptPriceMin <= '+convert(NVARCHAR(10), @Amount)+') and (nShipOptPriceMax <= 0 or nShipOptPriceMax >= '+convert(NVARCHAR(10), @Amount)+')   
   and (nShipOptWeightMin <= 0 or nShipOptWeightMin <= '+convert(NVARCHAR(10), @Weight)+') and (nShipOptWeightMax <= 0 or nShipOptWeightMax >= '+convert(NVARCHAR(10), @Weight)+')    
   and ((opt.cCurrency Is Null) or (opt.cCurrency = '''') or (opt.cCurrency = '''+@Currency+''')) and  
   opt.nShipOptKey =   
   CASE WHEN '+convert(NVARCHAR(10), @userId)+'= 0 THEN (CASE WHEN (SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm  where perm.nShippingMethodId = opt.nShipOptKey   
           and perm.nDirId = '+convert(NVARCHAR(10), @NonAuthUsers)+'  and perm.nPermLevel = 1) > 0   
           or (SELECT COUNT(*) from tblCartShippingPermission perm where opt.nShipOptKey = perm.nShippingMethodId and perm.nPermLevel = 1) = 0  
              THEN opt.nShipOptKey  END ) WHEN '+convert(NVARCHAR(10), @userId)+' >0 THEN (CASE WHEN ((SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm Inner join  
          tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId    
          where perm.nShippingMethodId = opt.nShipOptKey and PermGroup.nDirChildId = '+convert(NVARCHAR(10), @userId)+' and perm.nPermLevel = 1) > 0  
  
          and not((SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm   
          Inner join tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId    
          where perm.nShippingMethodId = opt.nShipOptKey and PermGroup.nDirChildId = '+convert(NVARCHAR(10), @userId)+' and perm.nPermLevel = 0) > 0)  
  
          Or (SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm   
          where perm.nShippingMethodId = opt.nShipOptKey And perm.nDirId = '+convert(NVARCHAR(10), @AuthUsers)+' And perm.nPermLevel = 1) > 0   
  
          or (SELECT COUNT(*) from tblCartShippingPermission perm where opt.nShipOptKey = perm.nShippingMethodId and perm.nPermLevel = 1) = 0)  
  
          And opt.nShipOptKey not in ( select nShippingMethodId from tblCartShippingPermission perm   
          Inner join tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId    
           and  nPermLevel = 0  and PermGroup.nDirChildId = '+convert(NVARCHAR(10), @userId)+')  
  
         THEN opt.nShipOptKey END  
    )  
    END '  
  
  -- -------------------------------------------------------------------------
  -- Country filter: if @CountryList is supplied, restrict results to
  -- shipping locations whose short or full name appears in the list.
  -- @CountryList is expected to arrive already formatted as a SQL IN list,
  -- e.g. ('GB','US','DE').
  -- -------------------------------------------------------------------------
  IF @CountryList <> ''  
  BEGIN
	 SET @strCountryConditionQuery = 'AND ((loc.cLocationNameShort IN '+@CountryList+') or (loc.cLocationNameFull IN '+@CountryList+')) '  
  END  
          
  -- -------------------------------------------------------------------------
  -- Audit status and publish/expire date filter.
  -- nStatus > 0 = the shipping method record is active/published in the CMS.
  -- dPublishDate and dExpireDate comparisons use @dValidDate (defaults to today)
  -- to ensure only currently visible methods are returned.
  -- -------------------------------------------------------------------------
  SET @strEndConditionQuery= 'AND (tblAudit.nStatus >0) AND ((tblAudit.dPublishDate = 0) or (tblAudit.dPublishDate Is Null) or (tblAudit.dPublishDate <= '''+convert(NVARCHAR(50), @dValidDate)+'''))
             AND ((tblAudit.dExpireDate = 0) or (tblAudit.dExpireDate Is Null) or (tblAudit.dExpireDate >= '''+convert(NVARCHAR(50), @dValidDate)+'''))' 
			 
  -- @OrderByCondition is built but not used in the concatenated string;
  -- ORDER BY is embedded directly inside the final CTE wrapper instead.
  SET @OrderByCondition=' order by opt.nDisplayPriority, nShippingTotal' 

  -- -------------------------------------------------------------------------
  -- Temporary and table-variable storage for shipping group resolution.
  -- #ShippingGroupList accumulates all product category rows (of schema type
  -- @GroupType) associated with items in the current cart order.
  -- @ShippingGroupCatIDs holds the distinct category IDs that have at least one
  -- shipping method mapped to them via tblCartShippingProductCategoryRelations.
  -- -------------------------------------------------------------------------
   CREATE TABLE #ShippingGroupList (id BIGINT, nCatKey BIGINT,  cCatName NVARCHAR(250), cCatSchemaName NVARCHAR(250))    
   DECLARE @ShippingGroupCatIDs TABLE (nCatId INT); 

  -- Count top-level cart items only (nParentId = 0 excludes bundled child rows).
  Select @CartItemCount= count(nCartItemKey) from tblCartItem where nCartOrderId=@CartOrderId and nParentId=0
  
 
  -- -------------------------------------------------------------------------
  -- Populate #ShippingGroupList when a cart order is in scope.
  -- Two sets of rows are UNION'd together:
  --   1. Direct product rows: cart items that are standard content products,
  --      along with any shipping-group category they belong to.
  --   2. Parent product rows via SKU child items: when a cart item is a child
  --      SKU (cContentSchemaName = 'SKU'), the shipping group is resolved from
  --      the parent product via tblContentRelation. This ensures that SKU
  --      variants inherit the shipping group of their parent product.
  -- -------------------------------------------------------------------------
  IF @CartOrderId > 0  
  BEGIN  

	   INSERT INTO #ShippingGroupList
	   select i.nItemId as contentId, cpc.nCatKey, cpc.cCatName, cpc.cCatSchemaName  
	   from tblCartItem i 
	   left join tblContent p on i.nItemId = p.nContentKey  
	   left join tblAudit A ON p.nAuditId= A.nAuditKey   
	   left join tblCartCatProductRelations cpr on p.nContentKey = cpr.nContentId 
	   left join tblCartProductCategories CPC ON cpr.nCatId= cpc.nCatKey
	   where nCartOrderId=@CartOrderId and i.nItemId <> 0  
	   UNION
	   select cr.nContentParentId as contentId, cpc.nCatKey, cpc.cCatName, cpc.cCatSchemaName  
	   from tblCartItem i 
	   left join tblContent p on i.nItemId = p.nContentKey  
	   left Join tblContentRelation cr on i.nItemId = cr.nContentChildId
	   left join tblAudit A ON p.nAuditId= A.nAuditKey   
	   left join tblCartCatProductRelations cpr on cr.nContentParentId = cpr.nContentId 
	   left join tblCartProductCategories CPC ON cpr.nCatId= cpc.nCatKey  
	   where nCartOrderId=@CartOrderId and i.nItemId <> 0
		 and p.cContentSchemaName = 'SKU'  -- Only include parent shipping groups if the cart item is a child SKU
     
		-- Count how many items in the cart have a shipping group category of @GroupType.
		-- @ExistShippingGroupCount > 0 means at least one cart item belongs to a shipping group.
		SET @ExistShippingGroupCount = (select COUNT(*) AS ExistShippingGroupCount from #ShippingGroupList where cCatSchemaName = @GroupType) 
		-- Count items associated with a shipping group; compared against @CartItemCount
		-- to determine whether ALL or only SOME items belong to a shipping group.
		Select @GroupItemCount=count(id) from #ShippingGroupList where cCatSchemaName=@GroupType

		-- Populate @ShippingGroupCatIDs with the distinct category IDs (nRuleType=1 = Include rule)
		-- that have at least one shipping method mapped to them AND appear in the current
		-- cart's shipping group list.  Used to build the dynamic IN clause later.
		INSERT INTO @ShippingGroupCatIDs (nCatId)
	    select distinct cspc.nCatId from tblCartShippingMethods opt  inner join
		tblCartShippingProductCategoryRelations CSPC ON opt.nShipOptKey= CSPC.nShipOptId  
		INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId and CSPC.nRuleType=1 and cspc.nCatId in (select distinct nCatKey from #ShippingGroupList where cCatSchemaName = @GroupType)		

		-- Resolve the display name of the shipping group by finding the category name
		-- that corresponds to the highest-priority category ID in @ShippingGroupCatIDs.
		-- Written into every result row (nShippingGroup) when a group is active.
		SET @ShippingGroupName = (select top 1 cCatName from #ShippingGroupList where cCatSchemaName = @GroupType and nCatKey = (select top 1 nCatId from @ShippingGroupCatIDs order by 1 desc ))
	   -- select * from  @ShippingGroupCatIDs
	   --Select @GroupItemCount
			-- When every cart item belongs to a shipping group, inject additional JOINs
			-- directly into the dynamic SELECT so results are restricted to methods
			-- explicitly mapped to the products in this order (nRuleType = 1 = Include).
			-- When only SOME items belong to a group, these joins are omitted and
			-- the restriction is applied via an IN subquery on @strEndConditionQuery instead.
			if(@GroupItemCount=@CartItemCount)
			BEGIN
				SET @shippingGroupCondition ='INNER JOIN tblCartShippingProductCategoryRelations CSPC ON opt.nShipOptKey= CSPC.nShipOptId  
				INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId and cpr.nContentId in (select distinct id from #ShippingGroupList)'  
				SET @shippingGroupRuleTypeCondition =' AND CSPC.nRuleType = 1'  
			END
		-- Build a comma-separated string of category IDs (e.g. '3,7,12') from
		-- @ShippingGroupCatIDs for embedding in the dynamic SQL IN clause.
		-- STUFF removes the leading comma produced by FOR XML PATH.
		SELECT @ShippingGroupCatIDList =
		STUFF((
			SELECT DISTINCT ',' + CAST(nCatId AS NVARCHAR)
			FROM @ShippingGroupCatIDs
			FOR XML PATH(''), TYPE
		).value('.', 'NVARCHAR(MAX)'), 1, 1, '');

	
  END
    
  -- -------------------------------------------------------------------------
  -- Promo code setup (runs whenever a promo code is present).
  -- Parses the XML stored in tblCartDiscountRules.cAdditionalXML to extract
  -- the comma-separated list of shipping method IDs that should be free
  -- (cFreeShippingMethods element). These IDs are split and loaded into
  -- @FreeShippingOption for use in the MERGE later.
  -- @ValidShippingOptions is declared here as a staging table so that the
  -- MERGE can update costs before the final SELECT is returned.
  -- -------------------------------------------------------------------------
  IF @PromoCode <> '' OR @PromoCode Is Not NULL  
  BEGIN
        DECLARE @FreeShippingOption AS TABLE (ID BIGINT)  
		DECLARE @GroupId  as nvarchar(500)  
		Select @GroupId= CONVERT(XML, cAdditionalXML).value('(/cFreeShippingMethods)[1]', 'varchar(100)') from tblCartDiscountRules where cDiscountCode=@PromoCode  
		INSERT INTO @FreeShippingOption  
		Select * from String_Split(@GroupId,',')  
  
		DECLARE @ValidShippingOptions AS TABLE (nShipOptKey INT,cCurrency NVARCHAR(10), cShipOptName NVARCHAR(255),cShipOptForeignRef NVARCHAR(255),cShipOptCarrier NVARCHAR(100),  
		cShipOptTime NVARCHAR(50),cShipOptTandC ntext, nShipOptCost money,nShipOptPercentage float,  
		nShipOptQuantMin float, nShipOptQuantMax float ,nShipOptWeightMin float ,nShipOptWeightMax float, nShipOptPriceMin float ,nShipOptPriceMax float,  
		nShipOptHandlingPercentage float, nShipOptHandlingFixedCost float,  
		nShipOptTaxRate float, nAuditId int, nDisplayPriority int , bCollection bit ,nShipOptCat int, nShippingTotal float ,NonDiscountedShippingCost float, nShippingGroup NVARCHAR(500), bOverrideForWholeOrder INT, nShipOptWeightOverageUnit float, nShipOptWeightOverageRate float,cLocationNameShort NVARCHAR(100))  
  END    
 
  
  -- =========================================================================
  -- BRANCH A: At least one cart item belongs to a shipping group.
  -- The available shipping methods must be restricted to those mapped to the
  -- active shipping group categories.  This IN filter is appended to
  -- @strEndConditionQuery regardless of whether all or only some items
  -- are in a group (the sub-branches produce identical SQL here; the
  -- difference in behaviour comes from the @shippingGroupCondition JOIN
  -- that was conditionally set above when GroupItemCount = CartItemCount).
  -- =========================================================================
  IF @ExistShippingGroupCount>0  
  BEGIN 
	-- Partial match: only some cart items have a shipping group.
	-- Restriction is enforced via the IN subquery on @strEndConditionQuery.
	IF(@GroupItemCount<> @CartItemCount)
	BEGIN
			
		    SET @strEndConditionQuery=@strEndConditionQuery + 'And opt.nShipOptKey in ( select distinct CSPC.nShipOptId from tblCartShippingProductCategoryRelations CSPC   
			INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId 
			INNER JOIN tblCartShippingMethods csm on CSPC.nShipOptId = csm.nShipOptKey 
			AND  CSPC.nRuleType = 1 and CSPC.nCatId in (' + @ShippingGroupCatIDList + ')) '
	END
	-- Full match: every cart item belongs to a shipping group.
	-- The @shippingGroupCondition JOIN is also active (set above), so the
	-- IN subquery here adds a secondary safeguard at the category level.
	ELSE
	BEGIN

			SET @strEndConditionQuery=@strEndConditionQuery + 'And opt.nShipOptKey in ( select distinct CSPC.nShipOptId from tblCartShippingProductCategoryRelations CSPC   
			INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId 
			INNER JOIN tblCartShippingMethods csm on CSPC.nShipOptId = csm.nShipOptKey 
			AND  CSPC.nRuleType = 1 and CSPC.nCatId in (' + @ShippingGroupCatIDList + ')) '
	END

	-- Assemble the full dynamic SELECT using all accumulated fragment strings.
	SET @strMainQuery = CONCAT (@strFirstQuery,@shippingGroupCondition,@strSecondQuery, @strCountryConditionQuery ,@strEndConditionQuery, @shippingGroupRuleTypeCondition)   
	-- Wrap in a CTE to apply the bOverrideForWholeOrder priority logic:
	-- If ANY returned method has bOverrideForWholeOrder = 1, show ONLY those
	-- methods (they supersede all standard options for the whole order).
	-- If NO method has the override flag, return all valid methods normally.
	SET @strMainQuery = ';WITH ShippingOptions AS (
						 '+@strMainQuery+'   
						)
						SELECT * FROM (SELECT * FROM ShippingOptions WHERE EXISTS (SELECT 1 FROM ShippingOptions so WHERE so.bOverrideForWholeOrder = 1) AND bOverrideForWholeOrder = 1
									UNION
									SELECT * FROM ShippingOptions WHERE NOT EXISTS (SELECT 1 FROM ShippingOptions so WHERE so.bOverrideForWholeOrder = 1)
						) AS FinalResult
						ORDER BY nDisplayPriority, nShippingTotal'


	--If promocode applied and promocode contains free shipping method then return that free shipping method with 0.00 cost  
	-- Execute query, then MERGE to zero out the cost of promo-granted free methods,
	-- preserving the original cost in NonDiscountedShippingCost for display purposes.
	IF @PromoCode <> '' AND @PromoCode Is Not NULL  
	BEGIN  

			INSERT INTO @ValidShippingOptions  
			exec (@strMainQuery)     

			MERGE @ValidShippingOptions T1  
			USING @FreeShippingOption T2  
			ON T1.nShipOptKey = T2.ID       
			WHEN MATCHED THEN  
			UPDATE SET NonDiscountedShippingCost = T1.nShipOptCost, nShippingTotal = 0.00, nShipOptCost = 0.00;   

			IF(@ShippingGroupName <> '')
			BEGIN
				-- Update shipping group name in final table if group exists
				UPDATE @ValidShippingOptions SET nShippingGroup = @ShippingGroupName
				SELECT * from @ValidShippingOptions  
			END

	END  
	ELSE  
	BEGIN  

			-- Execute Query without promocode 
			IF(@ShippingGroupName <> '')
			BEGIN

					INSERT INTO @ValidShippingOptions  
					exec (@strMainQuery)   
					-- Update shipping group name in final table if group exists
					UPDATE @ValidShippingOptions SET nShippingGroup = @ShippingGroupName
					SELECT * from @ValidShippingOptions  
			END
			ELSE
			BEGIN

					exec (@strMainQuery)  
			END			

	END  

	DROP TABLE #ShippingGroupList  
  END
  -- =========================================================================
  -- BRANCH B: No cart items belong to a shipping group (@ExistShippingGroupCount = 0).
  -- Standard shipping: return all methods that pass the range/permission/audit
  -- filters, without any shipping group restriction.
  -- However, if no items have any group association at all (@GroupItemCount = 0),
  -- explicitly exclude methods flagged as bOverrideForWholeOrder = 1 that are
  -- mapped to a shipping group category — those override methods are only
  -- meaningful in the context of a shipping group and should not appear here.
  -- =========================================================================
  ELSE  
  BEGIN 

		IF(@GroupItemCount=0)
		BEGIN
			-- Exclude any method that is mapped to a shipping group category
			-- with bOverrideForWholeOrder = 1, as those are reserved for group-based orders.
				SET @strEndConditionQuery=@strEndConditionQuery + 'And opt.nShipOptKey not in ( select distinct CSPC.nShipOptId from tblCartShippingProductCategoryRelations CSPC   
				INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId 
				INNER JOIN tblCartShippingMethods csm on CSPC.nShipOptId = csm.nShipOptKey 
				AND csm.bOverrideForWholeOrder = 1) '
		END
  
		-- Assemble the standard (non-group) dynamic query.
		-- No @shippingGroupCondition or @shippingGroupRuleTypeCondition is added here.
		SET @strMainQuery = CONCAT (@strFirstQuery,@strSecondQuery, @strCountryConditionQuery ,@strEndConditionQuery)   
		print (@strMainQuery)  -- Debug output; logs the assembled SQL to the messages pane.
		-- Apply the same CTE / bOverrideForWholeOrder logic as Branch A.
		SET @strMainQuery = 'WITH ShippingOptions AS (
						 '+@strMainQuery+'     
						)
						SELECT * FROM (SELECT * FROM ShippingOptions WHERE EXISTS (SELECT 1 FROM ShippingOptions so WHERE so.bOverrideForWholeOrder = 1) AND bOverrideForWholeOrder = 1
									UNION
									SELECT * FROM ShippingOptions WHERE NOT EXISTS (SELECT 1 FROM ShippingOptions so WHERE so.bOverrideForWholeOrder = 1)
						) AS FinalResult
						ORDER BY nDisplayPriority, nShippingTotal'

		-- If a promo code granting free shipping is present, execute into the staging
		-- table variable and MERGE to zero the cost of the matching methods before
		-- returning the result. Otherwise, execute and return directly.
		IF @PromoCode <> '' AND @PromoCode Is Not NULL  
		BEGIN  

			INSERT INTO @ValidShippingOptions  
			execute (@strMainQuery)     

			MERGE @ValidShippingOptions T1  
			USING @FreeShippingOption T2  
			ON T1.nShipOptKey = T2.ID       
			WHEN MATCHED THEN  
			UPDATE SET NonDiscountedShippingCost = T1.nShipOptCost, nShippingTotal = 0.00, nShipOptCost = 0.00;   

			SELECT  * from @ValidShippingOptions  
		END  
		ELSE  
		BEGIN  
			-- Execute Query without promocode  			
			exec (@strMainQuery)  
		END  
  END
  
 
  
END  
          

