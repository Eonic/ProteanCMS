
/*-------------------------------------------------------------------------------------------------------------
	Created By			:	Nita Dubal
	Created On			:	21 MARCH, 2023
	Last Modified By	:	Nita Dubal
	Last Modified On	:	10 May 2023
	Input Variables		:										
	Output Parameters	:	None
	Resultsets			:	-
	Sample Execution	:	EXECUTE [dbo].[spGetValidShippingOptions] 499584,40,0,0,'GBP',0,4,5,'(''United Kingdom'',''Local'',''Global'')','31-Mar-2023', 'test',''	-- For normal condition
	                        EXECUTE [dbo].[spGetValidShippingOptions] 0,40,0,0,'GBP',3324,4,5,'(''United Kingdom'',''Local'',''Global'')','21-Mar-2023' 	-- User specific
							EXECUTE [dbo].[spGetValidShippingOptions] 499586,20,0,0,'GBP',0,4,5,'(''United Kingdom'',''Local'',''Global'')','21-Mar-2023','' -- Dropship shipping group
							EXECUTE [dbo].[spGetValidShippingOptions] 499574,129,0,0,'GBP',0,4,5,'(''United Kingdom'',''Local'',''Global'')','30-Mar-2023'   --x50 shipping group
	
	Purpose				:	
	Revision			:	None
----------------------------------------------------------------------------------------------------------------*/



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

		CREATE TABLE #ShippingGroupList (id BIGINT, nCatKey BIGINT,  cCatName NVARCHAR(250), cCatSchemaName NVARCHAR(250)) 

		IF @CartOrderId > 0
		BEGIN
			
			InSERT INTO #ShippingGroupList
			select i.nItemId as contentId, cpc.nCatKey, cpc.cCatName, cpc.cCatSchemaName
			from tblCartItem i left join tblContent p on i.nItemId = p.nContentKey
			left join tblAudit A ON p.nAuditId= A.nAuditKey 
			left join tblCartCatProductRelations cpr on p.nContentKey = cpr.nContentId left join tblCartProductCategories CPC ON cpr.nCatId= cpc.nCatKey
			where nCartOrderId=@CartOrderId and i.nItemId <> 0
			
			SET @ExistShippingGroupCount = (select COUNT(*) AS ExistShippingGroupCount from #ShippingGroupList where cCatSchemaName = @GroupType)

			SET @shippingGroupCondition ='INNER JOIN tblCartShippingProductCategoryRelations CSPC ON opt.nShipOptKey= CSPC.nShipOptId
			INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId and cpr.nContentId in (select distinct id from #ShippingGroupList) '

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

				SET @shippingGroupCondition ='INNER JOIN tblCartShippingProductCategoryRelations CSPC ON opt.nShipOptKey= CSPC.nShipOptId
			    INNER JOIN tblCartCatProductRelations cpr on CSPC.nCatId= cpr.nCatId and cpr.nContentId in (select distinct id from #ShippingGroupList)'

			END
		END
		

		SET @strFirstQuery= 'select opt.*, dbo.fxn_shippingTotal(opt.nShipOptKey,'+convert(NVARCHAR(10), @Amount)+','+convert(NVARCHAR(10), @Quantity)+','+convert(NVARCHAR(10), @Weight)+') as nShippingTotal,0.00 AS NonDiscountedShippingCost  from tblCartShippingLocations Loc 
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
								      THEN opt.nShipOptKey 			   
				  
									  END )  
			 
			 WHEN '+convert(NVARCHAR(10), @userId)+' >0 THEN (CASE WHEN ((SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm Inner join
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

		
	    SET @shippingGroupRuleTypeCondition =' AND CSPC.nRuleType = 1'

		IF @PromoCode <> '' OR @PromoCode Is Not NULL
	    BEGIN
			    CREATE TABLE #FreeShippingOption (ID BIGINT)
				DECLARE @GroupId  as nvarchar(500)
				Select @GroupId= CONVERT(XML, cAdditionalXML).value('(/cFreeShippingMethods)[1]', 'varchar(100)') from tblCartDiscountRules where cDiscountCode=@PromoCode
				INSERT INTO #FreeShippingOption
				Select * from String_Split(@GroupId,',')

				CREATE TABLE #ValidShippingOptions (nShipOptKey INT,cCurrency NVARCHAR(10),	cShipOptName NVARCHAR(255),cShipOptForeignRef NVARCHAR(255),cShipOptCarrier NVARCHAR(100),
				cShipOptTime NVARCHAR(50),cShipOptTandC ntext,	nShipOptCost money,nShipOptPercentage float,
				nShipOptQuantMin float,	nShipOptQuantMax float	,nShipOptWeightMin float	,nShipOptWeightMax float,	nShipOptPriceMin float	,nShipOptPriceMax float,
				nShipOptHandlingPercentage float,	nShipOptHandlingFixedCost float,
				nShipOptTaxRate float,	nAuditId int,	nDisplayPriority int ,	bCollection bit	,nShipOptCat int,	nShippingTotal float	,NonDiscountedShippingCost float)
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

			 SET @strMainQuery = CONCAT (@strFirstQuery,@shippingGroupCondition,@strSecondQuery, @strCountryConditionQuery ,@strEndConditionQuery, @shippingGroupRuleTypeCondition, @OrderByCondition) 
			 
			 --If promocode applied and promocode contains free shipping method then return that free shipping method with 0.00 cost

			 IF @PromoCode <> '' OR @PromoCode Is Not NULL
			 BEGIN
				
				INSERT INTO #ValidShippingOptions
				EXECUTE (@strMainQuery)			

				MERGE #ValidShippingOptions T1
				USING #FreeShippingOption T2
				ON T1.nShipOptKey = T2.ID			  
				WHEN MATCHED THEN
					UPDATE SET NonDiscountedShippingCost = T1.nShipOptCost, nShippingTotal = 0.00, nShipOptCost = 0.00; 

				SELECT * from #ValidShippingOptions
			 END
			 ELSE
			 BEGIN
				-- Execute Query without promocode
				EXEC (@strMainQuery)
			 END

			 DROP TABLE #ShippingGroupList
		END
		ELSE
		BEGIN		

			 SET @strMainQuery = CONCAT (@strFirstQuery,@strSecondQuery, @strCountryConditionQuery ,@strEndConditionQuery,@OrderByCondition) 
			 
			  --If promocode applied and promocode contains free shipping method then return that free shipping method with 0.00 cost

			 IF @PromoCode <> '' OR @PromoCode Is Not NULL
			 BEGIN
				
				INSERT INTO #ValidShippingOptions
				EXECUTE (@strMainQuery)			

				MERGE #ValidShippingOptions T1
				USING #FreeShippingOption T2
				ON T1.nShipOptKey = T2.ID			  
				WHEN MATCHED THEN
					UPDATE SET NonDiscountedShippingCost = T1.nShipOptCost, nShippingTotal = 0.00, nShipOptCost = 0.00; 

				SELECT * from #ValidShippingOptions
			 END
			 ELSE
			 BEGIN
				-- Execute Query without promocode
				EXEC (@strMainQuery)
			 END
		END

		
        
END
