
 CREATE PROCEDURE [dbo].[spGetValidShippingOptions]
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
  Declare @OverrideForWholeOrder NVARCHAR(MAX); 
  Declare @ShippingGroupName NVARCHAR(MAX) ='' 
  DECLARE @ShippingGroupCatIDList NVARCHAR(MAX);


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
  
  IF @CountryList <> ''  
  BEGIN  
	 SET @strCountryConditionQuery = 'AND ((loc.cLocationNameShort IN '+@CountryList+') or (loc.cLocationNameFull IN '+@CountryList+')) '  
  END  
          
  SET @strEndConditionQuery= 'AND (tblAudit.nStatus >0) AND ((tblAudit.dPublishDate = 0) or (tblAudit.dPublishDate Is Null) or (tblAudit.dPublishDate <= '''+convert(NVARCHAR(50), @dValidDate)+'''))  
             AND ((tblAudit.dExpireDate = 0) or (tblAudit.dExpireDate Is Null) or (tblAudit.dExpireDate >= '''+convert(NVARCHAR(50), @dValidDate)+'''))' 
			 
  SET @OrderByCondition=' order by opt.nDisplayPriority, nShippingTotal' 
 
  
   CREATE TABLE #ShippingGroupList (id BIGINT, nCatKey BIGINT,  cCatName NVARCHAR(250), cCatSchemaName NVARCHAR(250))    
   DECLARE @ShippingGroupCatIDs TABLE (nCatId INT); 

  --get cart item count of an order.
  Select @CartItemCount= count(nCartItemKey) from tblCartItem where nCartOrderId=@CartOrderId and nParentId=0 
  
 
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
     
	    SET @ExistShippingGroupCount = (select COUNT(*) AS ExistShippingGroupCount from #ShippingGroupList where cCatSchemaName = @GroupType) 
		Select @GroupItemCount=count(id) from #ShippingGroupList where cCatSchemaName=@GroupType

		--Add new table for getting shipping group and there delivery options with override flag data    
		
		INSERT INTO @ShippingGroupCatIDs (nCatId)
	    select distinct cspc.nCatId from tblCartShippingMethods opt  inner join
		tblCartShippingProductCategoryRelations CSPC ON opt.nShipOptKey= CSPC.nShipOptId  
		INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId and CSPC.nRuleType=1 and cspc.nCatId in (select distinct nCatKey from #ShippingGroupList where cCatSchemaName = @GroupType)		

        SET @ShippingGroupName = (select top 1 cCatName from #ShippingGroupList where cCatSchemaName = @GroupType and nCatKey = (select top 1 nCatId from @ShippingGroupCatIDs order by 1 desc ))
	   -- select * from  @ShippingGroupCatIDs
	   --Select @GroupItemCount
			if(@GroupItemCount=@CartItemCount)
			BEGIN
				SET @shippingGroupCondition ='INNER JOIN tblCartShippingProductCategoryRelations CSPC ON opt.nShipOptKey= CSPC.nShipOptId  
				INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId and cpr.nContentId in (select distinct id from #ShippingGroupList)'  
				SET @shippingGroupRuleTypeCondition =' AND CSPC.nRuleType = 1'  
			END  
		SELECT @ShippingGroupCatIDList = 
		STUFF((
			SELECT DISTINCT ',' + CAST(nCatId AS NVARCHAR)
			FROM @ShippingGroupCatIDs
			FOR XML PATH(''), TYPE
		).value('.', 'NVARCHAR(MAX)'), 1, 1, '');

	
  END
    
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
 
  
  IF @ExistShippingGroupCount>0  
  BEGIN 
    IF(@GroupItemCount<> @CartItemCount)
	BEGIN
			
		    SET @strEndConditionQuery=@strEndConditionQuery + 'And opt.nShipOptKey in ( select distinct CSPC.nShipOptId from tblCartShippingProductCategoryRelations CSPC   
			INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId 
			INNER JOIN tblCartShippingMethods csm on CSPC.nShipOptId = csm.nShipOptKey 
			AND  CSPC.nRuleType = 1 and CSPC.nCatId in (' + @ShippingGroupCatIDList + ')) '
	END
	ELSE
	BEGIN
			
			SET @strEndConditionQuery=@strEndConditionQuery + 'And opt.nShipOptKey in ( select distinct CSPC.nShipOptId from tblCartShippingProductCategoryRelations CSPC   
			INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId 
			INNER JOIN tblCartShippingMethods csm on CSPC.nShipOptId = csm.nShipOptKey 
			AND  CSPC.nRuleType = 1 and CSPC.nCatId in (' + @ShippingGroupCatIDList + ')) '
	END

	
    SET @strMainQuery = CONCAT (@strFirstQuery,@shippingGroupCondition,@strSecondQuery, @strCountryConditionQuery ,@strEndConditionQuery, @shippingGroupRuleTypeCondition)   
    SET @strMainQuery = ';WITH ShippingOptions AS (
			             '+@strMainQuery+'   
						)
						SELECT * FROM (SELECT * FROM ShippingOptions WHERE EXISTS (SELECT 1 FROM ShippingOptions so WHERE so.bOverrideForWholeOrder = 1) AND bOverrideForWholeOrder = 1
									UNION
									SELECT * FROM ShippingOptions WHERE NOT EXISTS (SELECT 1 FROM ShippingOptions so WHERE so.bOverrideForWholeOrder = 1)
						) AS FinalResult
						ORDER BY nDisplayPriority, nShippingTotal'
							

    --If promocode applied and promocode contains free shipping method then return that free shipping method with 0.00 cost  
  
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
  ELSE  
  BEGIN 
  
		IF(@GroupItemCount=0)
		BEGIN
			
				SET @strEndConditionQuery=@strEndConditionQuery + 'And opt.nShipOptKey not in ( select distinct CSPC.nShipOptId from tblCartShippingProductCategoryRelations CSPC   
				INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId 
				INNER JOIN tblCartShippingMethods csm on CSPC.nShipOptId = csm.nShipOptKey 
				AND csm.bOverrideForWholeOrder = 1) '
		END
  
        SET @strMainQuery = CONCAT (@strFirstQuery,@strSecondQuery, @strCountryConditionQuery ,@strEndConditionQuery)   
        print (@strMainQuery)  
		SET @strMainQuery = 'WITH ShippingOptions AS (
			             '+@strMainQuery+'     
						)
						SELECT * FROM (SELECT * FROM ShippingOptions WHERE EXISTS (SELECT 1 FROM ShippingOptions so WHERE so.bOverrideForWholeOrder = 1) AND bOverrideForWholeOrder = 1
									UNION
									SELECT * FROM ShippingOptions WHERE NOT EXISTS (SELECT 1 FROM ShippingOptions so WHERE so.bOverrideForWholeOrder = 1)
						) AS FinalResult
						ORDER BY nDisplayPriority, nShippingTotal'

		 --If promocode applied and promocode contains free shipping method then return that free shipping method with 0.00 cost    
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
          

