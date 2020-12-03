SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[spCheckDiscounts]  
 @PromoCodeEntered VARCHAR(15)  
 ,@UserGroupIds VARCHAR(50)  
 ,@CartOrderId INT  
 ,@CartOrderDate Date 
AS  
BEGIN  
  
  
 SELECT tblCartDiscountRules.nDiscountKey  
  ,tblCartDiscountRules.nDiscountForeignRef  
  ,tblCartDiscountRules.cDiscountName  
  ,tblCartDiscountRules.cDiscountCode  
  ,tblCartDiscountRules.bDiscountIsPercent  
  ,tblCartDiscountRules.nDiscountCompoundBehaviour  
  ,tblCartDiscountRules.nDiscountValue  
  ,tblCartDiscountRules.nDiscountMinPrice  
  ,tblCartDiscountRules.nDiscountMinQuantity  
  ,tblCartDiscountRules.nDiscountCat  
  ,convert(nvarchar(max),tblCartDiscountRules.cAdditionalXML) as cAdditionalXML  
  ,tblCartDiscountRules.nAuditId  
  ,tblCartCatProductRelations.nContentId  
  ,tblCartDiscountRules.nDiscountCodeType  
  ,tblCartDiscountRules.cDiscountUserCode  
  ,ci.nCartItemKey  
  ,CASE   
   WHEN @PromoCodeEntered != ''  
    THEN dbo.fxn_checkDiscountCode(tblCartDiscountRules.nDiscountKey, @PromoCodeEntered)  
   ELSE 0  
   END AS [CodeUsedId]  
 FROM tblCartCatProductRelations  
 INNER JOIN tblCartDiscountProdCatRelations ON tblCartCatProductRelations.nCatId = tblCartDiscountProdCatRelations.nProductCatId  
 INNER JOIN tblCartDiscountRules  
 INNER JOIN tblCartDiscountDirRelations ON tblCartDiscountRules.nDiscountKey = tblCartDiscountDirRelations.nDiscountId  
 INNER JOIN tblAudit ON tblCartDiscountRules.nAuditId = tblAudit.nAuditKey ON tblCartDiscountProdCatRelations.nDiscountId = tblCartDiscountRules.nDiscountKey   
 INNER JOIN tblCartItem ci ON ci.nItemId = tblCartCatProductRelations.nContentId  
  AND ci.nCartOrderId = @CartOrderId WHERE (tblAudit.nStatus = 1)  
  AND (tblAudit.dExpireDate IS NULL OR tblAudit.dExpireDate >= @CartOrderDate)  
  AND (tblAudit.dPublishDate IS NULL OR tblAudit.dPublishDate <=@CartOrderDate)  
  AND (tblCartDiscountDirRelations.nDirId) IN (SELECT value FROM STRING_SPLIT(@UserGroupIds, ','))  
  AND (SELECT COUNT(dr2.nDiscountDirRelationKey)  
   FROM tblCartDiscountDirRelations dr2  
   WHERE dr2.nDirId IN (SELECT value FROM STRING_SPLIT(@UserGroupIds, ',')) AND nDiscountKey = dr2.nDiscountId  AND dr2.nPermLevel = 0) = 0  
  AND (tblCartCatProductRelations.nContentId IN (select nItemId from tblCartItem where nCartOrderId=@CartOrderId))  
  AND ((tblCartDiscountRules.cDiscountUserCode = '' AND tblCartDiscountRules.nDiscountCodeType = 0)  
  -- IF @PromoCodeEntered != ''  
   OR (@PromoCodeEntered != ''  AND tblCartDiscountRules.cDiscountUserCode = @PromoCodeEntered AND tblCartDiscountRules.nDiscountCodeType IN (1,2))  
   OR (@PromoCodeEntered != '' AND dbo.fxn_checkDiscountCode(tblCartDiscountRules.nDiscountKey, @PromoCodeEntered) > 0 AND tblCartDiscountRules.nDiscountCodeType = 3)
   -- ELSE
   OR (@PromoCodeEntered = '' AND tblCartDiscountRules.nDiscountCodeType IN (1,2,3))  
   -- END IF
   )  

 Union  
-- With excluded group logic   
 SELECT DISTINCT tblCartDiscountRules.nDiscountKey  
 ,tblCartDiscountRules.nDiscountForeignRef  
 ,tblCartDiscountRules.cDiscountName  
 ,tblCartDiscountRules.cDiscountCode  
 ,tblCartDiscountRules.bDiscountIsPercent  
 ,tblCartDiscountRules.nDiscountCompoundBehaviour  
 ,tblCartDiscountRules.nDiscountValue  
 ,tblCartDiscountRules.nDiscountMinPrice  
 ,tblCartDiscountRules.nDiscountMinQuantity  
 ,tblCartDiscountRules.nDiscountCat  
 ,CAST(tblCartDiscountRules.cAdditionalXML AS NVARCHAR(MAX)) AS cAdditionalXML  
 ,tblCartDiscountRules.nAuditId  
 ,ci.nItemId  
 ,tblCartDiscountRules.nDiscountCodeType  
 ,tblCartDiscountRules.cDiscountUserCode  
 ,ci.nCartItemKey  
 ,CASE WHEN @PromoCodeEntered != ''  
    THEN dbo.fxn_checkDiscountCode(tblCartDiscountRules.nDiscountKey, @PromoCodeEntered)  
   ELSE 0  
   END AS [CodeUsedId]  
 FROM tblCartDiscountRules  
 INNER JOIN tblCartDiscountDirRelations ON tblCartDiscountRules.nDiscountKey = tblCartDiscountDirRelations.nDiscountId  
 INNER JOIN tblAudit ON tblCartDiscountRules.nAuditId = tblAudit.nAuditKey  
 INNER JOIN tblCartItem ci ON ci.nCartOrderId = @CartOrderId and ci.nItemId!=0  
 WHERE (tblAudit.nStatus = 1) AND isnull(tblCartDiscountRules.bAllProductExcludeGroups, 0) = 1  
  AND (tblAudit.dExpireDate IS NULL OR tblAudit.dExpireDate >= @CartOrderDate)  
  AND (tblAudit.dPublishDate IS NULL OR tblAudit.dPublishDate <= @CartOrderDate)  
   AND (tblCartDiscountDirRelations.nDirId IN (SELECT value FROM STRING_SPLIT(@UserGroupIds, ',')))  
  AND (SELECT COUNT(dr2.nDiscountDirRelationKey) FROM tblCartDiscountDirRelations dr2  
   WHERE dr2.nDirId IN (Select value from STRING_SPLIT(@UserGroupIds,',')) AND nDiscountKey = dr2.nDiscountId  AND dr2.nPermLevel = 0) = 0  
  AND ((tblCartDiscountRules.cDiscountUserCode = '' AND tblCartDiscountRules.nDiscountCodeType = 0)  
  
   OR (tblCartDiscountRules.cDiscountUserCode = @PromoCodeEntered AND tblCartDiscountRules.nDiscountCodeType IN (1,2))  
   OR (tblCartDiscountRules.nDiscountCodeType = 3   
   AND dbo.fxn_checkDiscountCode(tblCartDiscountRules.nDiscountKey, @PromoCodeEntered) > 0 ))  
   AND ci.nItemId <> ( CASE WHEN (select count(c1.nContentId) from tblCartCatProductRelations c1    
   INNER JOIN tblCartDiscountProdCatRelations on c1.nCatId=tblCartDiscountProdCatRelations.nProductCatId   
      AND nContentId=(select top 1 nContentParentId from tblContentRelation where nContentChildId=ci.nItemId) AND tblCartDiscountProdCatRelations.nDiscountId=tblCartDiscountRules.nDiscountKey)<>0   
   THEN ci.nItemId     
   else 0 end)     
  
END