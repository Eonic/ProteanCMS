/****** Object:  StoredProcedure [dbo].[spGetValidShippingOptions]    Script Date: 13/06/2024 15:44:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 ALTER PROCEDURE [dbo].[spGetValidShippingOptions]
 -- Add the parameters for the stored procedure here  
 @CartOrderId BIGINT ,     
 @Amount FLOAT,  
 @Quantity BIGINT,  
 @Weight FLOAT,  
 @Currency NVARCHAR(3),  
 @userId BIGINT,  
 @AuthUsers BIGINT,  
 @NonAuthUsers BIGINT,  
 @CountryList NVARCHAR(1000),  
 @dValidDate Date = GetDate,   
 @PromoCode NVARCHAR(255),  
 @ProductId BIGINT,  
 @GroupType NVARCHAR(100) ='Shipping'
 
  
AS  
BEGIN  
 -- SET NOCOUNT ON added to prevent extra result sets from  
 -- interfering with SELECT statements.  
 SET NOCOUNT ON;  
     Declare @ExistShippingGroupCount INT =0    
     Declare @strMainQuery NVARCHAR(MAX) =''  
     Declare @strFirstQuery NVARCHAR(MAX) =''  
  Declare @strSecondQuery NVARCHAR(MAX) =''  
  Declare @strEndConditionQuery NVARCHAR(MAX) =''  
  Declare @strCountryConditionQuery NVARCHAR(MAX) =''  
  Declare @shippingGroupCondition NVARCHAR(MAX) =''  
  Declare @shippingGroupRuleTypeCondition NVARCHAR(MAX) =''  
  Declare @MainWhereCondition NVARCHAR(MAX) =''  
  Declare @OrderByCondition NVARCHAR(MAX) =''  
  Declare @CartItemCount As Int=0
  Declare @GroupItemCount As int=0
  Declare @bOverrideForWholeOrder INT =0  
  Declare @ShippingGroupCatID INT =0   
  Declare @ShippingGroupName NVARCHAR(MAX) =''  
  
  CREATE TABLE #ShippingGroupList (id BIGINT, nCatKey BIGINT,  cCatName NVARCHAR(250), cCatSchemaName NVARCHAR(250))    
  DECLARE @DeliveryOptionsForOverride AS TABLE (nCatId BIGINT, bOverrideForWholeOrder bit)   

  --get cart item count of an order.
  Select @CartItemCount= count(nCartItemKey) from tblCartItem where nCartOrderId=@CartOrderId and nParentId=0
  --select @CartItemCount
 
  IF @CartOrderId > 0  
  BEGIN  
     
	   INSERT INTO #ShippingGroupList  
	   select i.nItemId as contentId, cpc.nCatKey, cpc.cCatName, cpc.cCatSchemaName  
	   from tblCartItem i left join tblContent p on i.nItemId = p.nContentKey  
	   left join tblAudit A ON p.nAuditId= A.nAuditKey   
	   left join tblCartCatProductRelations cpr on p.nContentKey = cpr.nContentId left join tblCartProductCategories CPC ON cpr.nCatId= cpc.nCatKey  
	   where nCartOrderId=@CartOrderId and i.nItemId <> 0 and i.nParentId = 0
	   --TS addition of i.nParentId = 0 prevents product options being considered
     
	    SET @ExistShippingGroupCount = (select COUNT(*) AS ExistShippingGroupCount from #ShippingGroupList where cCatSchemaName = @GroupType) 

		--Add new table for getting shipping group and there delivery options with override flag data
	    INSERT INTO @DeliveryOptionsForOverride  
	    select distinct cspc.nCatId,  opt.bOverrideForWholeOrder from tblCartShippingMethods opt  inner join
		tblCartShippingProductCategoryRelations CSPC ON opt.nShipOptKey= CSPC.nShipOptId  
		INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId and CSPC.nRuleType=1 and cspc.nCatId in (select distinct nCatKey from #ShippingGroupList where cCatSchemaName = @GroupType)

		SEt @bOverrideForWholeOrder = (select distinct bOverrideForWholeOrder from @DeliveryOptionsForOverride where bOverrideForWholeOrder = 1)
		SET @ShippingGroupCatID = (select distinct nCatId from @DeliveryOptionsForOverride where bOverrideForWholeOrder = 1)
        SET @ShippingGroupName = (select distinct cCatName from #ShippingGroupList where cCatSchemaName = @GroupType and nCatKey = (select distinct nCatId from @DeliveryOptionsForOverride))

	   --select * from #ShippingGroupList 
	    Select @GroupItemCount=count(id) from #ShippingGroupList where cCatSchemaName=@GroupType
	   --Select @GroupItemCount
			if(@GroupItemCount=@CartItemCount)
			BEGIN
				SET @shippingGroupCondition ='INNER JOIN tblCartShippingProductCategoryRelations CSPC ON opt.nShipOptKey= CSPC.nShipOptId  
				INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId and cpr.nContentId in (select distinct id from #ShippingGroupList)'  
				SET @shippingGroupRuleTypeCondition =' AND CSPC.nRuleType = 1'  
			END
  
  END  
  ELSE  
  BEGIN  
  IF @ProductId>0   
  BEGIN  
		InSERT INTO #ShippingGroupList  
		select distinct i.nItemId as contentId, cpc.nCatKey, cpc.cCatName, cpc.cCatSchemaName  
		from tblCartItem i left join tblContent p on i.nItemId = p.nContentKey  
		left join tblAudit A ON p.nAuditId= A.nAuditKey   
		LEFT JOin  tblContentRelation CR ON i.nItemId= CR.nContentChildId  
		left join tblCartCatProductRelations cpr on p.nContentKey = cpr.nContentId left join tblCartProductCategories CPC ON cpr.nCatId= cpc.nCatKey  
		where CR.nContentParentId=@ProductId and i.nItemId <> 0 and cCatSchemaName=@GroupType  
		--select * from #CartDetails   
  
		SET @ExistShippingGroupCount = (select COUNT(*) AS ExistShippingGroupCount from #ShippingGroupList where cCatSchemaName = @GroupType)  

		INSERT INTO @DeliveryOptionsForOverride  
		select distinct cspc.nCatId,  opt.bOverrideForWholeOrder from tblCartShippingMethods opt  inner join
		tblCartShippingProductCategoryRelations CSPC ON opt.nShipOptKey= CSPC.nShipOptId  
		INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId and CSPC.nRuleType=1 and cspc.nCatId in (select distinct nCatKey from #ShippingGroupList where cCatSchemaName = @GroupType)

		--Getting override flag value and group id and shiiping group name
		SEt @bOverrideForWholeOrder = (select distinct bOverrideForWholeOrder from @DeliveryOptionsForOverride where bOverrideForWholeOrder = 1)
		SET @ShippingGroupCatID = (select distinct nCatId from @DeliveryOptionsForOverride where bOverrideForWholeOrder = 1)
		SET @ShippingGroupName = (select distinct cCatName from #ShippingGroupList where cCatSchemaName = @GroupType and nCatKey = (select distinct nCatId from @DeliveryOptionsForOverride))

		Select @GroupItemCount=count(id) from #ShippingGroupList where cCatSchemaName=@GroupType

		if(@GroupItemCount=@CartItemCount)
		BEGIN
				SET @shippingGroupCondition ='INNER JOIN tblCartShippingProductCategoryRelations CSPC ON opt.nShipOptKey= CSPC.nShipOptId  
				INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId and cpr.nContentId in (select distinct id from #ShippingGroupList)'  
				SET @shippingGroupRuleTypeCondition =' AND CSPC.nRuleType = 1'  
		END
   END  
  END  
    

	

  
  SET @strFirstQuery= 'select DISTINCT opt.nShipOptKey,opt.cCurrency,opt.cShipOptName,opt.cShipOptForeignRef,opt.cShipOptCarrier,opt.cShipOptTime, CAST(opt.cShipOptTandC AS NVARCHAR(MAX)) 
,opt.nShipOptCost,opt.nShipOptPercentage, opt.nShipOptQuantMin, opt.nShipOptQuantMax, opt.nShipOptWeightMin, opt.nShipOptWeightMax,  
opt.nShipOptPriceMin, opt.nShipOptPriceMax, opt.nShipOptHandlingPercentage, opt.nShipOptHandlingFixedCost, opt.nShipOptTaxRate,  opt.bOverrideForWholeOrder, 
opt.nAuditId, opt.nDisplayPriority, opt.bCollection, opt.nShipOptCat, dbo.fxn_shippingTotal(opt.nShipOptKey,'+convert(NVARCHAR(10), @Amount)+','+convert(NVARCHAR(10), @Quantity)+','+convert(NVARCHAR(10), @Weight)+') as nShippingTotal,0.00 AS NonDiscountedShippingCost, '''' AS nShippingGroup  from tblCartShippingLocations Loc   
   Inner Join tblCartShippingRelations rel ON Loc.nLocationKey = rel.nShpLocId   
   Inner Join tblCartShippingMethods opt ON rel.nShpOptId = opt.nShipOptKey   
   INNER JOIN tblAudit ON opt.nAuditId = tblAudit.nAuditKey '  
    
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
		nShipOptTaxRate float, bOverrideForWholeOrder bit, nAuditId int, nDisplayPriority int , bCollection bit ,nShipOptCat int, nShippingTotal float ,NonDiscountedShippingCost float, nShippingGroup NVARCHAR(500))  
  END  
  
  IF @CountryList <> ''  
  BEGIN  
	 SET @strCountryConditionQuery = 'AND ((loc.cLocationNameShort IN '+@CountryList+') or (loc.cLocationNameFull IN '+@CountryList+')) '  
  END  
          
  SET @strEndConditionQuery= 'AND (tblAudit.nStatus >0) AND ((tblAudit.dPublishDate = 0) or (tblAudit.dPublishDate Is Null) or (tblAudit.dPublishDate <= '''+convert(NVARCHAR(50), @dValidDate)+'''))  
             AND ((tblAudit.dExpireDate = 0) or (tblAudit.dExpireDate Is Null) or (tblAudit.dExpireDate >= '''+convert(NVARCHAR(50), @dValidDate)+'''))' 
			 
  SET @OrderByCondition=' order by opt.nDisplayPriority, nShippingTotal'  
  
  IF @ExistShippingGroupCount>0  
  BEGIN  
  
    IF(@GroupItemCount<> @CartItemCount)
	BEGIN
			--SET @strEndConditionQuery=@strEndConditionQuery + 'And opt.nShipOptKey not in ( select distinct CSPC.nShipOptId from tblCartShippingProductCategoryRelations CSPC   
			--	 INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId  AND  CSPC.nRuleType = 1 ) '
			--END

		-- Add new if condition for checking if flag is true then override shipping delivery option and if not then show other delievry options.
		-- For ITB we can set override flag as true and for non ITB we can set False
		    IF(@bOverrideForWholeOrder = 1)
			BEGIN
					 SET @strEndConditionQuery=@strEndConditionQuery + 'And opt.nShipOptKey in ( select distinct CSPC.nShipOptId from tblCartShippingProductCategoryRelations CSPC   
					 INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId 
					 INNER JOIN tblCartShippingMethods csm on CSPC.nShipOptId = csm.nShipOptKey 
					 AND  CSPC.nRuleType = 1 and csm.bOverrideForWholeOrder = 1 and CSPC.nCatId =  '+convert(NVARCHAR(10), @ShippingGroupCatID)+') '
			END
			ELSE
			BEGIN
						SET @strEndConditionQuery=@strEndConditionQuery + 'And opt.nShipOptKey not in ( select distinct CSPC.nShipOptId from tblCartShippingProductCategoryRelations CSPC   
					 INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId 
					 INNER JOIN tblCartShippingMethods csm on CSPC.nShipOptId = csm.nShipOptKey 
					 AND  CSPC.nRuleType = 1 and csm.bOverrideForWholeOrder = 0 ) '
			END
	END

    SET @strMainQuery = CONCAT (@strFirstQuery,@shippingGroupCondition,@strSecondQuery, @strCountryConditionQuery ,@strEndConditionQuery, @shippingGroupRuleTypeCondition, @OrderByCondition)   
     
	

    --If promocode applied and promocode contains free shipping method then return that free shipping method with 0.00 cost  
  
    IF @PromoCode <> '' OR @PromoCode Is Not NULL  
    BEGIN  
			 --  print (@strMainQuery)
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
  ELSE  
  BEGIN 
  
		IF(@GroupItemCount=0)
		BEGIN
					--SET @strEndConditionQuery=@strEndConditionQuery + 'And opt.nShipOptKey not in ( select distinct CSPC.nShipOptId from tblCartShippingProductCategoryRelations CSPC   
					--INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId  AND  CSPC.nRuleType = 1 ) '
				
				-- Add new if condition for checking if flag is true then override shipping delivery option and if not then show other delievry options.
				-- For ITB we can set override flag as true and for non ITB we can set False
				IF(@bOverrideForWholeOrder = 1)
				BEGIN
							SET @strEndConditionQuery=@strEndConditionQuery + 'And opt.nShipOptKey in ( select distinct CSPC.nShipOptId from tblCartShippingProductCategoryRelations CSPC   
							INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId 
							INNER JOIN tblCartShippingMethods csm on CSPC.nShipOptId = csm.nShipOptKey 
							AND  CSPC.nRuleType = 1 and csm.bOverrideForWholeOrder = 1 and CSPC.nCatId =  '+convert(NVARCHAR(10), @ShippingGroupCatID)+') '
				END
				ELSE
				BEGIN
							SET @strEndConditionQuery=@strEndConditionQuery + 'And opt.nShipOptKey not in ( select distinct CSPC.nShipOptId from tblCartShippingProductCategoryRelations CSPC   
							INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId 
							INNER JOIN tblCartShippingMethods csm on CSPC.nShipOptId = csm.nShipOptKey 
							AND  CSPC.nRuleType = 1 and csm.bOverrideForWholeOrder = 0 ) '
				END
		END

  
        SET @strMainQuery = CONCAT (@strFirstQuery,@strSecondQuery, @strCountryConditionQuery ,@strEndConditionQuery,@OrderByCondition)   
     
		 --If promocode applied and promocode contains free shipping method then return that free shipping method with 0.00 cost  
  
		IF @PromoCode <> '' OR @PromoCode Is Not NULL  
		BEGIN  
		   --print (@strMainQuery)  
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
          
