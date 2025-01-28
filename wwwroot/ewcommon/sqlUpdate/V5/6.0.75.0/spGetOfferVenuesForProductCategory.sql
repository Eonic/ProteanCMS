          
          
            
CREATE PROCEDURE [dbo].[spGetOfferVenuesForProductCategory] --801       
@Category3Id INT                
  
AS                  
BEGIN                  
                          
 SET NOCOUNT ON;                  
                  
 DECLARE @OutputXML XML                  
 DECLARE @StructKey INT   
 DECLARE @cContentKeys VARCHAR(Max)  
                  
 SELECT @StructKey = nStructKey                  
 FROM [dbo].[tblContentStructure]                  
 WHERE nStructKey = @Category3Id      
   
 DECLARE @tempTble Table (                
            cContentKeyID varchar(25) NULL);      
  
INSERT INTO @tempTble (cContentKeyID)  
 SELECT c.nContentKey  
 FROM tblContent c  
 INNER JOIN tblContentLocation CL ON c.nContentKey = CL.nContentId  
 INNER JOIN tblAudit a ON c.nAuditId = a.nAuditKey  
 WHERE (  
   CL.nStructId = @Category3Id  
   AND a.nStatus = 1  
   AND (  
    a.dPublishDate IS NULL  
    OR a.dPublishDate = 0  
    OR a.dPublishDate <= CONVERT(DATE, GETDATE())  
    )  
   AND (  
    a.dExpireDate IS NULL  
    OR a.dExpireDate = 0  
    OR a.dExpireDate >= CONVERT(DATE, GETDATE())  
    )  
   )  
  AND cContentSchemaName = 'Product'  
 ORDER BY cContentSchemaName  
  ,cl.nDisplayOrder  
  
  
print @cContentKeys  
                
 --create table variable for content keys                
  DECLARE @cContentKeyID varchar(max) = Null ;                
  SET @cContentKeyID= @cContentKeys                
              
                
              
 SET @OutputXML = (                  
   SELECT                  
    --cContentXmlBrief,                             
    DISTINCT C.nContentKey                  
    ,cContentXmlBrief.value('(/Content/Name)[1]', 'varchar(500)') AS ProductName                  
    ,nLat AS Latitude                  
    ,nLong AS Longitude                  
 ,cContactCity as City                
 ,isnull(cContactCountry ,'United Kingdom') as Country                
    ,COALESCE(SKU.SKUPrice, CONVERT(DECIMAL(14, 2), ISNULL(NULLIF(C.cContentXmlDetail.value('(/Content/Prices/Price[@type="sale"]/text())[1]', 'VARCHAR(10)'), ''), 0))) AS ProductPrice                  
    -- ,cContentXmlBrief.value('(/Content/Prices/Price[@type="sale"])[1]', 'varchar(20)') AS ProductPrice                      
    ,cContentXmlBrief.value('(/Content/Images/img[@class="detail"]/@src)[1]', 'varchar(800)') AS ImagePath                  
    ,cContentXmlBrief.value('(/Content/ShortDescription)[1]', 'varchar(50)') + '...' AS ProductDescription       
 ,cContentXmlBrief.value('(/Content/LocationMap/@shortDescription)[1]', 'varchar(200)')  AS LocationDesc      
    ,CONCAT (                  
     '/experiences/'                  
     ,C.ProductUrl                  
     ) AS ProductUrl                  
   FROM [dbo].[tblContentLocation] L                  
   JOIN (                  
    SELECT c.nContentKey                  
     ,c.cContentSchemaName                  
     ,CAST(c.cContentXmlBrief AS XML) AS cContentXmlBrief                  
     ,CAST(c.cContentXmlDetail AS XML) AS cContentXmlDetail                  
     ,REPLACE(LOWER(c.cContentName), ' ', '-') AS ProductUrl                  
     ,cc.nLat                  
     ,cc.nLong                 
  ,cc.cContactCity                
  , cc.cContactCountry                
    FROM [tblContent] C                  
 INNER JOIN @tempTble t ON C.nContentKey = t.cContentKeyID      -- add join for getting matched contentkeys records                
    JOIN tblContentRelation cr ON cr.nContentParentId = c.nContentKey                  
    JOIN tblAudit acr ON acr.nAuditKey = cr.nAuditId                  
     AND acr.nStatus = 1                  
    JOIN tblContent cSku ON cr.nContentChildId = csku.nContentKey                  
    JOIN tblAudit acsku ON acsku.nAuditKey = cSku.nAuditId                  
     AND acsku.nStatus = 1                  
   JOIN [dbo].[tblITBSupplierOffer] so ON so.nSKUContentKey = cSku.nContentKey          
 join [intotheblue_demo].[dbo].[tblSupplierActivity] sa  on so.cSupplierOfferForiegnRef=sa.intSupplierActivityID and isnull(sa.blnArchived,0)=0        
    JOIN tblAudit aso ON aso.nAuditKey = so.nAuditId                  
     AND aso.nStatus = 1                  
    JOIN tblITBSupplierOfferVenues sov ON sov.nOfferId = so.nSupplierOfferKey  and isnull(so.bIsDeleted,0)=0                     
    JOIN tblCartContact cc ON cc.nContactKey = sov.nContactId --Join tblAudit acc on acc.nAuditKey=cc.nAuditId and acc.nStatus=1                
             
    JOIN [tblAudit] A ON A.nAuditKey = C.nAuditId                  
    WHERE A.nStatus = 1                  
    ) C ON C.nContentKey = L.nContentId                  
   OUTER APPLY (                  
    SELECT MIN(CONVERT(DECIMAL(14, 2), ISNULL(NULLIF(cast(SKU.cContentXmlDetail AS XML).value('(/Content/Prices/Price[@type="sale"]/text())[1]', 'VARCHAR(10)'), ''), 0))) AS SKUPrice                  
    FROM dbo.tblContentRelation CR                  
    JOIN [tblContent] SKU ON CR.nContentChildId = SKU.nContentKey             
 inner join tblAudit a on a.nAuditKey=sku.nAuditId and a.nStatus=1          
    WHERE CR.nContentParentId = C.nContentKey                  
     AND cContentSchemaName = 'SKU'                  
    ) SKU                  
   WHERE nStructId = @StructKey                  
    AND C.cContentSchemaName = 'Product'                  
   FOR XML PATH('OfferVenue')                  
    ,ROOT('OfferVenuesList')                  
   )                  
                  
 SELECT @OutputXML                  
END 